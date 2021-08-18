using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
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
    internal sealed class MethodWeaver
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
                    nextInstruction = ProcessAnchorMethod(instruction);
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

        private Instruction? ProcessAnchorMethod(Instruction instruction)
        {
            var anchorMethod = (GenericInstanceMethod)instruction.Operand;
            var interfaceType = anchorMethod.GenericArguments.First();
            var interfaceTypeDef = interfaceType.Resolve();
            if (!interfaceTypeDef.IsInterface)
                throw new InstructionWeavingException(instruction, "The method Base<T> requires that T is an interface type, but got " + interfaceTypeDef.FullName);

            var next = instruction.Next;
            if (next == null)
                throw NoInterfaceMethodInvocationException();

            if (IsStloc(next))
                throw NoInterfaceMethodInvocationException();

            if (IsCallAndPop(next) && !IsInterfaceMethodCandidate(next, interfaceTypeDef))
            {
                if (IsAnchorMethodCall(next))
                    throw new InvalidOperationException("The method Base<T> cannot be invoked followed by another Base<T>");
                else
                    throw NoInterfaceMethodInvocationException();
            }

            for (var p = next; p != null; p = p.Next)
            {
                if (!IsInterfaceMethodCandidate(p, interfaceTypeDef))
                    continue;

                var graph = Instructions.BuildGraph();
                var args = p.GetArgumentPushInstructions(Instructions, graph);
                var arg = args.First();

                if (arg != instruction)
                    continue;

                p = EmitBaseInvokeInstructions(instruction, interfaceTypeDef, p);
                return p.Next;
            }

            throw NoInterfaceMethodInvocationException();
        }

        private Instruction EmitBaseInvokeInstructions(Instruction anchor, TypeDefinition interfaceTypeDef, Instruction invokeInstruction)
        {
            var methodRef = (MethodReference)invokeInstruction.Operand;

            _il.Remove(anchor);

            if (interfaceTypeDef.IsEqualTo(methodRef.DeclaringType))
            {
                var method = methodRef.Resolve();
                EnsureNonAbstract(method);

                invokeInstruction.OpCode = OpCodes.Call;
                return invokeInstruction;
            }

            var interfaceDefaultMethod = interfaceTypeDef.GetInterfaceDefaultMethod(methodRef);
            EnsureNonAbstract(interfaceDefaultMethod);

            // use Calli instead of Call to avoid MethodAccessException
            var handle = _il.Locals.AddLocalVar(new LocalVarBuilder(Types.RuntimeMethodHandle));
            var ptr = _il.Locals.AddLocalVar(new LocalVarBuilder(Types.IntPtr));
            var callSite = new StandAloneMethodSigBuilder(CallingConventions.HasThis, interfaceDefaultMethod).Build();

            var to64 = _il.Create(OpCodes.Call, Methods.ToInt64);
            var to32 = _il.Create(OpCodes.Call, Methods.ToInt32);
            var calli = _il.Create(OpCodes.Calli, callSite);

            var instructions = new List<Instruction>(4)
            {
                _il.Create(OpCodes.Ldtoken, interfaceDefaultMethod),
                _il.Create(OpCodes.Stloc, handle),
                _il.Create(OpCodes.Ldloca, handle),
                _il.Create(OpCodes.Call, Methods.GetFunctionPointer),
                _il.Create(OpCodes.Stloc, ptr),
                _il.Create(OpCodes.Ldloca, ptr),
                _il.Create(OpCodes.Call, Methods.Is64BitProcess),
                _il.Create(OpCodes.Brfalse, to32),
                to64,
                _il.Create(OpCodes.Br, calli),
                to32,
                calli,
                Instruction.Create(OpCodes.Nop)
            };

            var cur = _il.InsertAfter(invokeInstruction, instructions);
            _il.Remove(invokeInstruction);
            return cur;
        }

        private static bool IsInterfaceMethodCandidate(Instruction instruction, TypeDefinition typeDef)
        {
            if (instruction.OpCode != OpCodes.Callvirt
                || instruction.Operand is not MethodReference methodRef)
                return false;

            return typeDef.IsAssignableTo(methodRef.DeclaringType);
        }

        private static bool IsCallAndPop(Instruction instruction)
        {
            return instruction.OpCode.FlowControl == FlowControl.Call && instruction.GetPopCount() > 0;
        }

        private static bool IsStloc(Instruction instruction)
        {
            switch (instruction.OpCode.Code)
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

        private static Exception NoInterfaceMethodInvocationException()
        {
            return new InvalidOperationException("The method Base<T> requires that an interface methods to be base-invoked with its return value fluently");
        }

        private static void EnsureNonAbstract(MethodDefinition method)
        {
            if (method.IsAbstract)
                throw new InvalidOperationException($"The abstract interface method {method.FullName} cannot be invoked");
        }
    }
}
