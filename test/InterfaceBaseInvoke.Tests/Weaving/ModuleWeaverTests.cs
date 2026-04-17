namespace InterfaceBaseInvoke.Tests.Weaving;

public class ModuleWeaverTests(ITestOutputHelper output)
{
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
                Assert.True(type.IsWeaverReferencedDeep(context), $"The type {type.FullName} from {module.Assembly.MainModule.Name} does not reference the weaver.");
            }
        }
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
            var tempFile = Path.GetTempFileName();
            File.Copy(module.Assembly.MainModule.FileName, tempFile, true);

            using var tempModule = ModuleDefinition.ReadModule(tempFile, new ReaderParameters { ReadWrite = true });

            output.WriteLine("ModuleWeaver is executing: " + tempModule.Assembly.MainModule.FileName);
            var wearer = new ModuleWeaver
            {
                ModuleDefinition = tempModule,
            };
            wearer.Execute();
        }
    }
}
