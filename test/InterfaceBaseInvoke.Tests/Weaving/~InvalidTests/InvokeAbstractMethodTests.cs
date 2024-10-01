using System.Linq;
using System.Threading.Tasks;
using InterfaceBaseInvoke.Tests.InvalidAssemblyToProcess;
using InterfaceBaseInvoke.Tests.Support;

namespace InterfaceBaseInvoke.Tests.Weaving
{
    public class InvokeAbstractMethodTests : ClassTestsBase
    {
        protected override string ClassName => nameof(InvokeAbstractMethodTestCases);

        [Fact]
        public void EmptyMethod_Invoke()
        {
            var error = ShouldHaveError(nameof(InvokeAbstractMethodTestCases.EmptyMethod_Invoke));
            error.ShouldContain("IHasEmptyMethod::Method");
            error.ShouldContain("The abstract interface method");
            error.ShouldContain("cannot be invoked");
        }

        [Fact]
        public void EmptyMethod_Invoke_MultiLevel()
        {
            var error = ShouldHaveError(nameof(InvokeAbstractMethodTestCases.EmptyMethod_Invoke_MultiLevel));
            error.ShouldContain("IHasEmptyMethod::Method");
            error.ShouldContain("The abstract interface method");
            error.ShouldContain("cannot be invoked");
        }
    }
}
