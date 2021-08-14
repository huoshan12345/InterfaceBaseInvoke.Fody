using System;
using System.Collections.Generic;
using System.Linq;
using Fody;
using InterfaceBaseInvoke.Fody.Extensions;
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
        private Collection<Instruction> Instructions => _method.Body.Instructions;

        public MethodWeaver(ModuleDefinition module, MethodDefinition method, ILogger log)
        {
            _module = module;
            _method = method;
            _il = new WeaverILProcessor(method);
            _log = new MethodWeaverLogger(log, _method);
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

            var stlocIndex = GetStlocIndex(instruction.Next);

            for (var p = instruction.Next; p != null; p = p.Next)
            {
                // prepare for next ProcessAnchorMethod
                if (nextInstruction != null && IsAnchorMethodCall(p))
                    nextInstruction = p;

                if (p.OpCode != OpCodes.Callvirt
                    || p.Operand is not MethodReference method)
                    continue;

                if (method.DeclaringType.FullName != interfaceTypeDef.FullName)
                    continue;

                var arg = _il.GetArgumentPushInstructionsInSameBasicBlock(p).First();

                if (arg == instruction)
                {
                    p.OpCode = OpCodes.Call; // change callvirt to call
                    continue;
                }

                if (stlocIndex >= 0 && stlocIndex == GetLdlocIndex(arg))
                {
                    p.OpCode = OpCodes.Call; // change callvirt to call
                    continue;
                }

            }
            _il.Remove(instruction);
        }

        private static int GetStlocIndex(Instruction instruction)
        {
            return instruction.OpCode.Code switch
            {
                Code.Stloc or Code.Stloc_S => (int)instruction.Operand,
                Code.Stloc_0 => 0,
                Code.Stloc_1 => 1,
                Code.Stloc_2 => 2,
                Code.Stloc_3 => 3,
                _ => -1
            };
        }

        private static int GetLdlocIndex(Instruction instruction)
        {
            return instruction.OpCode.Code switch
            {
                Code.Ldloc or Code.Ldloc_S => (int)instruction.Operand,
                Code.Ldloc_0 => 0,
                Code.Ldloc_1 => 1,
                Code.Ldloc_2 => 2,
                Code.Ldloc_3 => 3,
                _ => -1
            };
        }
    }
}
