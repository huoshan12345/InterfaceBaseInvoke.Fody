using System;
using System.Reflection;
using Fody;
using InterfaceBaseInvoke.Fody;
using InterfaceBaseInvoke.Tests.AssemblyToProcess;
using InterfaceBaseInvoke.Tests.InvalidAssemblyToProcess;
using InterfaceBaseInvoke.Tests.Support;
using Xunit;

namespace InterfaceBaseInvoke.Tests.Weaving
{
    public abstract class ClassTestsBase
    {
        protected static readonly string VerifiableAssembly = typeof(AssemblyToProcessReference).Assembly.GetName().Name!;
        protected static readonly string InvalidAssembly = typeof(InvalidAssemblyToProcessReference).Assembly.GetName().Name!;

        protected virtual bool NetStandard => false;
        protected abstract string ClassName { get; }

        protected dynamic GetInstance()
        {
            return NetStandard
                ? StandardAssemblyToProcessFixture.TestResult.GetInstance($"{VerifiableAssembly}.{ClassName}")
                : AssemblyToProcessFixture.TestResult.GetInstance($"{VerifiableAssembly}.{ClassName}");
        }

        protected string ShouldHaveError(string methodName)
            => InvalidAssemblyToProcessFixture.ShouldHaveError($"{InvalidAssembly}.{ClassName}", methodName, true);

        protected void CheckEqual(string methodName)
        {
            object obj = GetInstance();
            var method = obj.GetType().GetMethod(methodName);

            Assert.NotNull(method);

            var result = method!.IsStatic
                ? method.Invoke(null, null)
                : method.Invoke(obj, null);

            Assert.NotNull(result);
            Assert.True(result is ValueTuple<string, string>);

            var (expected, actual) = ((string, string))result!;

            Assert.Equal(expected, actual);
        }
    }
}
