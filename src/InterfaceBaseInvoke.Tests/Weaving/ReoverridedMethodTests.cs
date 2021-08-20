using InterfaceBaseInvoke.Tests.AssemblyToProcess;
using Xunit;

namespace InterfaceBaseInvoke.Tests.Weaving
{
    public class ReoverridedMethodTests : ClassTestsBase
    {
        protected override string ClassName => nameof(ReoverridedMethodTestCases);

        [Fact]
        public void ReoverrideMethod_InvokeTwice()
        {
            string value = GetInstance().ReoverrideMethod_InvokeTwice();
            Assert.Equal("IHasOverridedMethod.Method.1.a----IHasReoverridedMethod.Method.2.b", value);
        }

        [Fact]
        public void Property_InvokeTwice()
        {
            string value = GetInstance().Property_InvokeTwice();
            Assert.Equal("IHasOverridedMethod.Property----IHasReoverridedMethod.Property", value);
        }
    }

    public class ReoverridedMethodTestsStandard : ReoverridedMethodTests
    {
        protected override bool NetStandard => true;
    }
}
