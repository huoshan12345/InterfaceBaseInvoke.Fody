﻿using InterfaceBaseInvoke.Tests.AssemblyToProcess;

#pragma warning disable 618

namespace InterfaceBaseInvoke.Tests.Support;

public static class AssemblyToProcessFixture
{
    public static TestResult TestResult { get; }

    public static ModuleDefinition OriginalModule { get; }
    public static ModuleDefinition ResultModule { get; }

    static AssemblyToProcessFixture()
    {
        (TestResult, OriginalModule, ResultModule) = Process<AssemblyToProcessReference>();
    }

    internal static (TestResult testResult, ModuleDefinition originalModule, ModuleDefinition resultModule) Process<T>()
    {
        var assemblyPath = FixtureHelper.IsolateAssembly<T>();

        var weavingTask = new GuardedWeaver();

        var testResult = weavingTask.ExecuteTestRun(
            assemblyPath,
            ignoreCodes: new[]
            {
                "0x801312da" // VLDTR_E_MR_VARARGCALLINGCONV
            },
            writeSymbols: true,
            beforeExecuteCallback: BeforeExecuteCallback,
            runPeVerify: false
        );

        using var assemblyResolver = new TestAssemblyResolver();

        var readerParams = new ReaderParameters(ReadingMode.Immediate)
        {
            ReadSymbols = true,
            AssemblyResolver = assemblyResolver
        };

        var originalModule = ModuleDefinition.ReadModule(assemblyPath, readerParams);
        var resultModule = ModuleDefinition.ReadModule(testResult.AssemblyPath, readerParams);

        return (testResult, originalModule, resultModule);
    }

    internal static void BeforeExecuteCallback(ModuleDefinition module)
    {
        // This reference is added by Fody, it's not supposed to be there
        module.AssemblyReferences.RemoveWhere(i => string.Equals(i.Name, "System.Private.CoreLib", StringComparison.OrdinalIgnoreCase));
    }
}
