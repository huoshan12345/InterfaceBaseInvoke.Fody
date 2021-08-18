using System.Reflection;
using Fody;
using InterfaceBaseInvoke.Fody;
using InterfaceBaseInvoke.Tests.AssemblyToProcess;

namespace InterfaceBaseInvoke.Tests
{
    public abstract class ClassTestsBase
    {
        protected static readonly Assembly VerifiableAssembly = typeof(InvokeInClassTestCases).Assembly;
        protected const string UnverifiableAssembly = "InterfaceBaseInvoke.Tests.UnverifiableAssemblyToProcess";
        protected const string InvalidAssembly = "InterfaceBaseInvoke.Tests.InvalidAssemblyToProcess";

        protected static readonly TestResult TestResult;
        protected abstract string ClassName { get; }

        static ClassTestsBase()
        {
            var weavingTask = new ModuleWeaver();
            TestResult = weavingTask.ExecuteTestRun(VerifiableAssembly.Location, false);
        }

        protected dynamic GetInstance()
        {
            return TestResult.GetInstance(ClassName);
        }
    }
}
