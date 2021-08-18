using System.Linq;
using Fody;
using InterfaceBaseInvoke.Fody;
using InterfaceBaseInvoke.Tests.InvalidAssemblyToProcess;
using Mono.Cecil;

namespace InterfaceBaseInvoke.Tests.Support
{
    public static class InvalidAssemblyToProcessFixture
    {
        public static TestResult TestResult { get; }

        public static ModuleDefinition ResultModule { get; }

        static InvalidAssemblyToProcessFixture()
        {
            var weavingTask = new ModuleWeaver();
            TestResult = weavingTask.ExecuteTestRun(
                FixtureHelper.IsolateAssembly<InvalidAssemblyToProcessReference>(),
                false,
                beforeExecuteCallback: AssemblyToProcessFixture.BeforeExecuteCallback
            );

            using var assemblyResolver = new TestAssemblyResolver();

            ResultModule = ModuleDefinition.ReadModule(TestResult.AssemblyPath, new ReaderParameters(ReadingMode.Immediate)
            {
                AssemblyResolver = assemblyResolver
            });
        }

        public static string ShouldHaveError(string className, string methodName, bool sequencePointRequired)
        {
            var expectedMessagePart = $" {className}::{methodName}(";
            var errorMessage = TestResult.Errors.SingleOrDefault(err => err.Text.Contains(expectedMessagePart));
            errorMessage.ShouldNotBeNull();

            if (sequencePointRequired)
                errorMessage!.SequencePoint.ShouldNotBeNull();

            return errorMessage!.Text;
        }

        public static void ShouldHaveErrorInType(string className, string nestedTypeName)
        {
            var expectedMessagePart = $" {className}/{nestedTypeName}";
            TestResult.Errors.ShouldAny(err => err.Text.Contains(expectedMessagePart));
        }
    }
}
