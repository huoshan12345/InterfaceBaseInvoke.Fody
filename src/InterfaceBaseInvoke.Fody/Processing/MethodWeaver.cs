using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Fody;
using InterfaceBaseInvoke.Fody.Extensions;
using InterfaceBaseInvoke.Fody.Models;
using InterfaceBaseInvoke.Fody.Support;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;

namespace InterfaceBaseInvoke.Fody.Processing
{
    internal class MethodWeaver
    {
        private const string AnchorMethodDeclaringTypeName = "InterfaceBaseInvoke.ObjectExtension";
        private const string AnchorMethodName = "Base";

        private readonly ModuleDefinition _module;
        private readonly MethodDefinition _method;
        private readonly MethodWeaverLogger _log;
        private readonly WeaverILProcessor _il;
        private readonly References _references;
        private Collection<Instruction> Instructions => _method.Body.Instructions;
        private TypeReferences Types => _references.Types;
        private MethodReferences Methods => _references.Methods;

        public MethodWeaver(ModuleDefinition module, MethodDefinition method, ILogger log)
        {
            _module = module;
            _method = method;
            _il = new WeaverILProcessor(method);
            _log = new MethodWeaverLogger(log, _method);
            _references = new References(module);
        }

        public static bool NeedsProcessing(ModuleDefinition module, MethodDefinition method)
            => HasLibReference(module, method, out _);

        private static bool HasLibReference(ModuleDefinition module, MethodDefinition method, out Instruction? refInstruction)
        {
            refInstruction = null;

            if (method.IsWeaverAssemblyReferenced(module))
                return true;

            if (!method.HasBody)
                return false;

            if (method.Body.HasVariables && method.Body.Variables.Any(i => i.VariableType.IsWeaverAssemblyReferenced(module)))
                return true;

            foreach (var instruction in method.Body.Instructions)
            {
                refInstruction = instruction;

                switch (instruction.Operand)
                {
                    case MethodReference methodRef when methodRef.IsWeaverAssemblyReferenced(module):
                    case TypeReference typeRef when typeRef.IsWeaverAssemblyReferenced(module):
                    case FieldReference fieldRef when fieldRef.IsWeaverAssemblyReferenced(module):
                    case CallSite callSite when callSite.IsWeaverAssemblyReferenced(module):
                        return true;
                }
            }

            refInstruction = null;
            return false;
        }

        public void Process()
        {
            try
            {
                ProcessImpl();
            }
            catch (InstructionWeavingException ex)
            {
                throw new WeavingException(_log.QualifyMessage(ex.Message, ex.Instruction))
                {
                    SequencePoint = ex.Instruction.GetInputSequencePoint(_method)
                };
            }
            catch (WeavingException ex)
            {
                throw new WeavingException(_log.QualifyMessage(ex.Message))
                {
                    SequencePoint = ex.SequencePoint
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error occured while processing method {_method.FullName}: {ex.Message}", ex);
            }
        }

        private static bool IsAnchorMethodCall(Instruction instruction)
        {
            if (instruction.OpCode != OpCodes.Call
                || instruction.Operand is not GenericInstanceMethod method)
                return false;

            return method.DeclaringType.FullName == AnchorMethodDeclaringTypeName
                   && method.Name == AnchorMethodName;
        }

        private void ProcessImpl()
        {
            var instruction = Instructions.FirstOrDefault();
            Instruction? nextInstruction;

            for (; instruction != null; instruction = nextInstruction)
            {
                nextInstruction = instruction.Next;

                if (!IsAnchorMethodCall(instruction))
                    continue;

                try
                {
                    ProcessAnchorMethod(instruction, out nextInstruction);
                }
                catch (InstructionWeavingException)
                {
                    throw;
                }
                catch (WeavingException ex)
                {
                    throw new InstructionWeavingException(instruction, _log.QualifyMessage(ex.Message, instruction));
                }
                catch (Exception ex)
                {
                    throw new InstructionWeavingException(instruction, $"Unexpected error occured while processing method {_method.FullName} at instruction {instruction}: {ex}");
                }
            }
        }

        private void ProcessAnchorMethod(Instruction instruction, out Instruction? nextInstruction)
        {
            nextInstruction = null;
            var anchorMethod = (GenericInstanceMethod)instruction.Operand;
            var interfaceType = anchorMethod.GenericArguments.First();
            var interfaceTypeDef = interfaceType.Resolve();
            if (!interfaceTypeDef.IsInterface)
                throw new InstructionWeavingException(instruction, "The method Base<T> requires that T is an interface type, but got " + interfaceTypeDef.FullName);

            if (IsStloc(instruction.Next))
                throw new InstructionWeavingException(instruction, "The interface methods to be base-invoked requires that they are called fluently after Base<T>.");

            for (var p = instruction.Next; p != null; p = p.Next)
            {
                // prepare for next ProcessAnchorMethod
                if (nextInstruction == null && IsAnchorMethodCall(p))
                    nextInstruction = p;

                if (p.OpCode != OpCodes.Callvirt
                    || p.Operand is not MethodReference methodRef)
                    continue;

                if (!interfaceTypeDef.IsAssignableTo(methodRef.DeclaringType))
                    continue;

                var arg = _il.GetArgumentPushInstructionsInSameBasicBlock(p).First();

                if (arg != instruction)
                    continue;

                if (interfaceTypeDef.IsEqualTo(methodRef.DeclaringType))
                {
                    p.OpCode = OpCodes.Call;
                }
                else
                {
                    var handle = _il.Locals.AddLocalVar(new LocalVarBuilder(Types.RuntimeMethodHandle));
                    var ptr = _il.Locals.AddLocalVar(new LocalVarBuilder(Types.IntPtr));
                    var method = interfaceTypeDef.GetInterfaceDefaultMethod(methodRef);
                    var callSite = new StandAloneMethodSigBuilder(CallingConventions.HasThis, method).Build();

                    var to64 = _il.Create(OpCodes.Call, Methods.ToInt64);
                    var to32 = _il.Create(OpCodes.Call, Methods.ToInt32);
                    var calli = _il.Create(OpCodes.Calli, callSite);

                    var instructions = new List<Instruction>(4)
                    {
                        _il.Create(OpCodes.Ldtoken, method),
                        Instruction.Create(OpCodes.Stloc, handle),
                        Instruction.Create(OpCodes.Ldloca, handle),
                        _il.Create(OpCodes.Call, Methods.GetFunctionPointer),
                        Instruction.Create(OpCodes.Stloc, ptr),
                        Instruction.Create(OpCodes.Ldloca, ptr),
                        _il.Create(OpCodes.Call, Methods.Is64BitProcess),
                        _il.Create(OpCodes.Brfalse, to32),
                        to64,
                        _il.Create(OpCodes.Br, calli),
                        to32,
                        calli,
                    };

                    var cur = _il.InsertAfter(p, instructions);
                    _il.Remove(p);
                    p = cur;
                }
            }
            _il.Remove(instruction);
        }

        private static bool IsStloc(Instruction? instruction)
        {
            switch (instruction?.OpCode.Code)
            {
                case Code.Stloc:
                case Code.Stloc_S:
                case Code.Stloc_0:
                case Code.Stloc_1:
                case Code.Stloc_2:
                case Code.Stloc_3:
                    return true;
                default:
                    return false;
            }
        }
    }
}
