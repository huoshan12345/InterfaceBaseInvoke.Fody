using Fody;
using InterfaceBaseInvoke.Fody.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InterfaceBaseInvoke.Fody.Support;

namespace InterfaceBaseInvoke.Fody
{
    public sealed class ModuleWeaver : BaseModuleWeaver
    {
        private readonly Logger _log;

        public ModuleWeaver()
        {
            _log = new Logger(this);
        }

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

                        _log.Debug($"Processing: {method.FullName}");
                        new MethodWeaver(ModuleDefinition, method, _log).Process();
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
