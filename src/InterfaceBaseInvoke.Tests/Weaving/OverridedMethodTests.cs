using InterfaceBaseInvoke.Tests.AssemblyToProcess;
using Xunit;

namespace InterfaceBaseInvoke.Tests.Weaving
{
    public class OverridedMethodTests : ClassTestsBase
    {
        protected override string ClassName => nameof(OverridedMethodTestCases);
        
        [Fact]
        public void OverridedMethod_Invoke()
        {
            string value = GetInstance().OverridedMethod_Invoke();
            Assert.Equal("IHasOverridedMethod.Method.29.HasOverridedMethod.OverridedMethod_Invoke", value);
        }

        [Fact]
        public void OverridedMethod_InvokeTwice()
        {
            string value = GetInstance().OverridedMethod_InvokeTwice();
            Assert.Equal("IHasOverridedMethod.Method.1.a----IHasOverridedMethod.Method.2.b", value);
        }

        [Fact]
        public void Property_Invoke()
        {
            string value = GetInstance().Property_Invoke();
            Assert.Equal("IHasOverridedMethod.Property", value);
        }
    }

    public class OverridedMethodTestsStandard : OverridedMethodTests
    {
        protected override bool NetStandard => true;
    }
}
