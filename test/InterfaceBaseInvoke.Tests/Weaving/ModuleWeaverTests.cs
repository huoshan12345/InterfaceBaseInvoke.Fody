using InterfaceBaseInvoke.Tests.Support;
using Xunit.Abstractions;
#pragma warning disable CS0618 // Type or member is obsolete

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
}
