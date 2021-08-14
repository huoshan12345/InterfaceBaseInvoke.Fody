﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Fody;
using InterfaceBaseInvoke.Fody.Extensions;
using InterfaceBaseInvoke.Fody.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace InterfaceBaseInvoke.Fody.Processing
{
    internal class MethodLocals
    {
        private readonly Dictionary<string, VariableDefinition> _localsByName = new();
        private readonly List<VariableDefinition> _localsByIndex = new();

        public MethodLocals(MethodDefinition method, IEnumerable<LocalVarBuilder> locals)
        {
            foreach (var local in locals)
            {
                var localVar = local.Build();

                if (local.Name != null)
                {
                    if (_localsByName.ContainsKey(local.Name))
                        throw new WeavingException($"Local {local.Name} is already defined");

                    _localsByName.Add(local.Name, localVar);
                }

                _localsByIndex.Add(localVar);
                method.Body.Variables.Add(localVar);

                method.DebugInformation.Scope?.Variables.Add(new VariableDebugInformation(localVar, local.Name ?? $"InlineIL_{_localsByIndex.Count - 1}"));
            }
        }

        public VariableDefinition? TryGetByName(string name)
            => _localsByName.GetValueOrDefault(name);

        public static void MapMacroInstruction(MethodLocals? locals, Instruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Ldloc_0:
                    MapIndex(OpCodes.Ldloc, 0);
                    break;
                case Code.Ldloc_1:
                    MapIndex(OpCodes.Ldloc, 1);
                    break;
                case Code.Ldloc_2:
                    MapIndex(OpCodes.Ldloc, 2);
                    break;
                case Code.Ldloc_3:
                    MapIndex(OpCodes.Ldloc, 3);
                    break;
                case Code.Stloc_0:
                    MapIndex(OpCodes.Stloc, 0);
                    break;
                case Code.Stloc_1:
                    MapIndex(OpCodes.Stloc, 1);
                    break;
                case Code.Stloc_2:
                    MapIndex(OpCodes.Stloc, 2);
                    break;
                case Code.Stloc_3:
                    MapIndex(OpCodes.Stloc, 3);
                    break;
            }

            void MapIndex(OpCode opCode, int index)
            {
                var local = GetLocalByIndex(locals, index);
                instruction.OpCode = opCode;
                instruction.Operand = local;
            }
        }

        public static bool MapIndexInstruction(MethodLocals? locals, ref OpCode opCode, int index, [MaybeNullWhen(false)] out VariableDefinition result)
        {
            switch (opCode.Code)
            {
                case Code.Ldloc:
                case Code.Ldloc_S:
                case Code.Ldloca:
                case Code.Ldloca_S:
                case Code.Stloc:
                case Code.Stloc_S:
                {
                    result = GetLocalByIndex(locals, index);
                    return true;
                }

                default:
                    result = null;
                    return false;
            }
        }

        private static VariableDefinition GetLocalByIndex(MethodLocals? locals, int index)
        {
            if (locals == null)
                throw new WeavingException("No locals are defined");

            if (index < 0 || index >= locals._localsByIndex.Count)
                throw new WeavingException($"Local index {index} is out of range");

            return locals._localsByIndex[index];
        }
    }
}
