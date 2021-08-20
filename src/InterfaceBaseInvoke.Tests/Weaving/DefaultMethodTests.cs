using InterfaceBaseInvoke.Tests.AssemblyToProcess;
using Xunit;

namespace InterfaceBaseInvoke.Tests.Weaving
{
    public class DefaultMethodTests : ClassTestsBase
    {
        protected override string ClassName => nameof(DefaultMethodTestCases);

        [Fact]
        public void DefaultMethod_Invoke()
        {
            string value = GetInstance().DefaultMethod_Invoke();
            Assert.Equal("IHasDefaultMethod.Method.29.HasDefaultMethod.Invoke", value);
        }

        [Fact]
        public void DefaultMethod_InvokeTwice()
        {
            string value = GetInstance().DefaultMethod_InvokeTwice();
            Assert.Equal("IHasDefaultMethod.Method.1.a----IHasDefaultMethod.Method.2.b", value);
        }

        [Fact]
        public void Property_Invoke()
        {
            string value = GetInstance().Property_Invoke();
            Assert.Equal("IHasDefaultMethod.Property", value);
        }
    }

    public class DefaultMethodTestsStandard : DefaultMethodTests
    {
        protected override bool NetStandard => true;
    }
}
