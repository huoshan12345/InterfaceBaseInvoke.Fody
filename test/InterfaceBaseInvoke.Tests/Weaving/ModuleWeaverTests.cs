#pragma warning disable CS0618 // Type or member is obsolete

using System.IO;
using System.Linq;
using InterfaceBaseInvoke.Fody.Processing;
using MoreFodyHelpers.Processing;

namespace InterfaceBaseInvoke.Tests.Weaving;

public class ModuleWeaverTests
{
    private readonly ITestOutputHelper _output;

    public ModuleWeaverTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Execute_Test()
    {
        var modules = new[]
        {
            AssemblyToProcessFixture.OriginalModule,
            StandardAssemblyToProcessFixture.OriginalModule,
        };
        foreach (var module in modules)
        {
            _output.WriteLine("ModuleWeaver is executing: " + module.Assembly.MainModule.FileName);
            var wearer = new ModuleWeaver
            {
                ModuleDefinition = module,
                LogError = m => throw new InvalidOperationException(m),
                LogErrorPoint = (m, p) => throw new InvalidOperationException(m),
            };
            wearer.Execute();
        }
    }

    [Fact]
    public void IsWeaverReferenced_Test()
    {
        var modules = new[]
        {
            AssemblyToProcessFixture.OriginalModule,
            StandardAssemblyToProcessFixture.OriginalModule,
        };
        foreach (var module in modules)
        {
            using var context = module.CreateWeavingContext();
            foreach (var type in module.GetTypes().Where(m => m.Name.EndsWith("TestCases")))
            {
                Assert.True(type.IsWeaverReferencedDeep(context));
            }
        }
    }
}
