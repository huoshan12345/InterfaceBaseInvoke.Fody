using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace InterfaceBaseInvoke.Fody.Extensions
{
    public static class InstructionExtensions
    {
        public static SequencePoint? GetInputSequencePoint(this Instruction? instruction, MethodDefinition method)
        {
            if (instruction == null)
                return null;

            var sequencePoints = method.DebugInformation.HasSequencePoints
                ? method.DebugInformation.SequencePoints
                : Enumerable.Empty<SequencePoint>();

            return sequencePoints.LastOrDefault(sp => sp.Offset <= instruction.Offset);
        }
    }
}
