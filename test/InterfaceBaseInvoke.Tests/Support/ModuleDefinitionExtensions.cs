using System.IO;
using InterfaceBaseInvoke.Fody.Processing;
using MoreFodyHelpers.Processing;

namespace InterfaceBaseInvoke.Tests.Support;

public static class ModuleDefinitionExtensions
{
    public static ModuleWeavingContext CreateWeavingContext(this ModuleDefinition module)
    {
        var path = module.Assembly.MainModule.FileName;
        var dir = Path.GetDirectoryName(path);
        return new(module, WeaverAnchors.AssemblyName, dir);
    }
}
