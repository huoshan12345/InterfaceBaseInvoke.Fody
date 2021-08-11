using InterfaceBaseInvoke.Tests.AssemblyToProcess;
using Xunit;

namespace InterfaceBaseInvoke.Tests
{
    public class InvokeOutsideTests : ClassTestsBase
    {
        protected override string ClassName { get; } = typeof(InvokeOutsideTestCases).FullName!;

        [Fact]
        public void Call_Expression_Test()
        {
            var obj = GetInstance();
            Assert.Equal(1, obj.Call_Expression());
        }
    }
}
