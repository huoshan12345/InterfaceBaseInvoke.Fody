using System.Reflection;
using Fody;
using InterfaceBaseInvoke.Fody;
using InterfaceBaseInvoke.Tests.AssemblyToProcess;
using InterfaceBaseInvoke.Tests.Support;

namespace InterfaceBaseInvoke.Tests.Weaving
{
    public abstract class ClassTestsBase
    {
        protected static readonly string VerifiableAssembly = typeof(AssemblyToProcessReference).Assembly.GetName().Name!;
        protected const string UnverifiableAssembly = "InterfaceBaseInvoke.Tests.UnverifiableAssemblyToProcess";
        protected const string InvalidAssembly = "InterfaceBaseInvoke.Tests.InvalidAssemblyToProcess";

        protected virtual bool NetStandard => false;
        protected abstract string ClassName { get; }

        protected dynamic GetInstance()
        {
            return NetStandard
                ? StandardAssemblyToProcessFixture.TestResult.GetInstance($"{VerifiableAssembly}.{ClassName}")
                : AssemblyToProcessFixture.TestResult.GetInstance($"{VerifiableAssembly}.{ClassName}");
        }
    }
}
