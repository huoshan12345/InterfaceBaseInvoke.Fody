using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceBaseInvoke.Tests.InvalidAssemblyToProcess;
using InterfaceBaseInvoke.Tests.Support;
using Xunit;

namespace InterfaceBaseInvoke.Tests.Weaving
{
    public class InvokeAbstractMethodTests : ClassTestsBase
    {
        protected override string ClassName => nameof(InvokeAbstractMethodTestCases);

        [Fact]
        public void EmptyMethod_Call()
        {
            var error = ShouldHaveError(nameof(InvokeAbstractMethodTestCases.EmptyMethod_Call));
            error.ShouldContain("IEmptyMethod::Method");
            error.ShouldContain("The abstract interface method");
            error.ShouldContain("cannot be invoked");
        }

        [Fact]
        public void EmptyMethod_Call_MultiLevel()
        {
            var error = ShouldHaveError(nameof(InvokeAbstractMethodTestCases.EmptyMethod_Call_MultiLevel));
            error.ShouldContain("IEmptyMethod::Method");
            error.ShouldContain("The abstract interface method");
            error.ShouldContain("cannot be invoked");
        }
    }
}
