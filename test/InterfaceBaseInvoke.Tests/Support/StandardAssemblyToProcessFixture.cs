using InterfaceBaseInvoke.Tests.StandardAssemblyToProcess;

namespace InterfaceBaseInvoke.Tests.Support
{
    public static class StandardAssemblyToProcessFixture
    {
        public static TestResult TestResult { get; }

        public static ModuleDefinition OriginalModule { get; }
        public static ModuleDefinition ResultModule { get; }

        static StandardAssemblyToProcessFixture()
        {
            (TestResult, OriginalModule, ResultModule) = AssemblyToProcessFixture.Process<StandardAssemblyToProcessReference>();
        }
    }
}
