using InterfaceBaseInvoke.Tests.AssemblyToProcess;
using Xunit;

namespace InterfaceBaseInvoke.Tests.Weaving
{
    public class DefaultGenericMethodTests : ClassTestsBase
    {
        protected override string ClassName => nameof(DefaultGenericMethodTestCases);

        [Fact]
        public void Method_Invoke()
        {
            string value = GetInstance().Method_Invoke();
            Assert.Equal("IHasDefaultMethod.Method.29.Method_Invoke", value);
        }

        [Fact]
        public void Method_InvokeTwice()
        {
            string value = GetInstance().Method_InvokeTwice();
            Assert.Equal("IHasDefaultMethod.Method.1.a----IHasDefaultMethod.Method.2.b", value);
        }

        [Fact]
        public void GenericMethod_Invoke()
        {
            string value = GetInstance().GenericMethod_Invoke();
            Assert.Equal("IHasDefaultMethod.Method.29.DefaultGenericMethod.GenericMethod_Invoke.Int32", value);
        }

        [Fact]
        public void GenericMethod_InvokeTwice()
        {
            string value = GetInstance().GenericMethod_InvokeTwice();
            Assert.Equal("IHasDefaultMethod.Method.1.a.Int32----IHasDefaultMethod.Method.2.b.String", value);
        }
    }

    public class DefaultGenericMethodTestsStandard : DefaultMethodTests
    {
        protected override bool NetStandard => true;
    }
}
