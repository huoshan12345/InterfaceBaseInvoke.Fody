using Fody;
using InterfaceBaseInvoke.Fody.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InterfaceBaseInvoke.Fody
{
    public class ModuleWeaver : BaseModuleWeaver
    {
        public override void Execute()
        {
            foreach (var type in ModuleDefinition.GetTypes())
            {
                foreach (var method in type.Methods)
                {
                    try
                    {
                        if (!MethodWeaver.NeedsProcessing(ModuleDefinition, method))
                            continue;

                        WriteDebug($"Processing: {method.FullName}");
                    }
                    catch (WeavingException ex)
                    {
                        WriteError(ex.Message, ex.SequencePoint);
                    }
                }
            }
        }

        public override IEnumerable<string> GetAssembliesForScanning() => Enumerable.Empty<string>();
    }
}
