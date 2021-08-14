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
        public void Invoke_InPlace_Test()
        {
            var obj = GetInstance();
            Assert.Equal(4, obj.InPlace());
        }
    }
}
