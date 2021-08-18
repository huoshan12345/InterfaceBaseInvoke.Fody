using InterfaceBaseInvoke.Tests.AssemblyToProcess;
using Xunit;

namespace InterfaceBaseInvoke.Tests.Weaving
{
    public class InvokeOutsideTests : ClassTestsBase
    {
        protected override string ClassName => nameof(InvokeOutsideTestCases);

        [Fact]
        public void DefaultMethod_Call()
        {
            string value = GetInstance().DefaultMethod_Call();
            Assert.Equal("IDefaultMethod.Method.29.InheritIDefaultMethod.DefaultMethod_Call", value);
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
            Assert.Equal("IOverridedMethod.Method.29.InheritIOverridedMethod.OverridedMethod_Call", value);
        }

        [Fact]
        public void OverridedMethod_MutipleCall()
        {
            string value = GetInstance().OverridedMethod_MutipleCall();
            Assert.Equal("IOverridedMethod.Method.1.a----IOverridedMethod.Method.2.b", value);
        }

        [Fact]
        public void OverridedMethod_Call_Jump()
        {
            string value = GetInstance().OverridedMethod_Call_Jump();
            Assert.Equal("----IOverridedMethod.Method.1.a----", value);
        }

        [Fact]
        public void OverridedMethod_Call_JumpInInvocation()
        {
            string value = GetInstance().OverridedMethod_Call_JumpInInvocation();
            Assert.Equal("----IOverridedMethod.Method.11.aa----", value);
        }
    }

    public class InvokeOutsideTestsStandard : InvokeOutsideTests
    {
        protected override bool NetStandard => true;
    }
}
