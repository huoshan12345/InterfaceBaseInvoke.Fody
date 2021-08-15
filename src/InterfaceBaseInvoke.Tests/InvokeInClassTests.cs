using System;
using System.Reflection;
using InterfaceBaseInvoke.Tests.AssemblyToProcess;
using Xunit;

namespace InterfaceBaseInvoke.Tests
{
    public class InvokeInClassTests : ClassTestsBase
    {
        protected override string ClassName { get; } = typeof(InvokeInClassTestCases).FullName!;

        [Fact]
        public void DefaultMethod_Call()
        {
            string value = GetInstance().DefaultMethod_Call();
            Assert.Equal("IDefaultMethod.Method.29.InheritIDefaultMethod.Call", value);
        }

        [Fact]
        public void DefaultMethod_MutipleCall()
        {
            string value = GetInstance().DefaultMethod_MutipleCall();
            Assert.Equal("IDefaultMethod.Method.1.a----IDefaultMethod.Method.2.b", value);
        }

        [Fact]
        public void OverridedMethod_Call()
        {
            string value = GetInstance().OverridedMethod_Call();
            Assert.Equal("IDefaultMethod.Method.29.InheritIDefaultMethod.Call", value);
        }

        [Fact]
        public void OverridedMethod_MutipleCall()
        {
            string value = GetInstance().OverridedMethod_MutipleCall();
            Assert.Equal("IDefaultMethod.Method.29.InheritIDefaultMethod.Call", value);
        }
    }
}
