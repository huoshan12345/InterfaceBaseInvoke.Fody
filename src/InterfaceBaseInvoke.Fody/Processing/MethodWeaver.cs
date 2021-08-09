﻿using System;
using System.Collections.Generic;
using System.Linq;
using Fody;
using InterfaceBaseInvoke.Fody.Extensions;
using InterfaceBaseInvoke.Fody.Support;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace InterfaceBaseInvoke.Fody.Processing
{
    internal class MethodWeaver
    {
        private const string TargetMethodName = "InterfaceBaseInvoke.ObjectExtension.Base";

        private readonly ModuleDefinition _module;
        private readonly MethodDefinition _method;
        private readonly MethodWeaverLogger _log;

        public MethodWeaver(ModuleDefinition module, MethodDefinition method, ILogger log)
        {
            _module = module;
            _method = method;
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

        private void ProcessImpl()
        {
            var instruction = _method.Body.Instructions.FirstOrDefault();

            while (instruction != null)
            {
                var nextInstruction = instruction.Next;

                if (instruction.OpCode == OpCodes.Call && instruction.Operand is MethodReference)
                {
                    try
                    {
                        ProcessMethodCallFirstPass(instruction, ref nextInstruction);
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

                instruction = nextInstruction;
            }
        }

        private void ProcessMethodCallFirstPass(Instruction instruction, ref Instruction? nextInstruction)
        {
            // First pass: Process helper methods. Only removes existing instructions.

            var calledMethod = (MethodReference)instruction.Operand;

            if (calledMethod.FullName != TargetMethodName)
                return;


        }
    }
}
