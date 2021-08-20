using InterfaceBaseInvoke.Tests.AssemblyToProcess;
using Xunit;

namespace InterfaceBaseInvoke.Tests.Weaving
{
    public class OverridedGenericMethodTests : ClassTestsBase
    {
        protected override string ClassName => nameof(OverridedGenericMethodTestCases);

        [Fact]
        public void Method_Invoke()
        {
            string value = GetInstance().Method_Invoke();
            Assert.Equal("IHasOverridedGenericMethod.Method.29.HasOverridedGenericMethod.Method_Invoke", value);
        }

        [Fact]
        public void Method_InvokeTwice()
        {
            string value = GetInstance().Method_InvokeTwice();
            Assert.Equal("IHasOverridedGenericMethod.Method.1.a----IHasOverridedGenericMethod.Method.2.b", value);
        }

        [Fact]
        public void GenericMethod_Invoke()
        {
            string value = GetInstance().GenericMethod_Invoke();
            Assert.Equal("IHasOverridedGenericMethod.Method.29.HasOverridedGenericMethod.GenericMethod_Invoke.Int32", value);
        }

        [Fact]
        public void GenericMethod_InvokeTwice()
        {
            string value = GetInstance().GenericMethod_InvokeTwice();
            Assert.Equal("IHasOverridedGenericMethod.Method.1.a.Int32----IHasOverridedGenericMethod.Method.2.b.String", value);
        }
    }

    public class OverridedGenericMethodTestsStandard : OverridedGenericMethodTests
    {
        protected override bool NetStandard => true;
    }
}
