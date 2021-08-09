using InterfaceBaseInvoke.Tests.AssemblyToProcess;
using Xunit;

namespace InterfaceBaseInvoke.Tests
{
    public class InvokeInClassTests : ClassTestsBase
    {
        protected override string ClassName { get; } = nameof(InvokeInClassTestCases);

        [Fact]
        public void Call_Test()
        {
            var obj = GetInstance<InvokeInClassTestCases>();

            Assert.Equal(1, obj.Call());
        }
    }
}
