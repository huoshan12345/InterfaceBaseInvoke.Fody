using InterfaceBaseInvoke.Tests.InvalidAssemblyToProcess;

namespace InterfaceBaseInvoke.Tests.Weaving
{
    public class InvokeMethodFluentlyTests : ClassTestsBase
    {
        protected override string ClassName => nameof(InvokeMethodFluentlyTestCases);

        [InvalidAssemblyOnlyDebugFact]
        public void InvokeMethod_WithVariable()
        {
            var error = ShouldHaveError(nameof(InvokeMethodFluentlyTestCases.InvokeMethod_WithVariable));
            error.ShouldContain("The method Base<T> requires that an interface methods to be base-invoked with its return value fluently");
        }

        [Fact]
        public void InvokeMethod_NotDeclaredByInterface()
        {
            var error = ShouldHaveError(nameof(InvokeMethodFluentlyTestCases.InvokeMethod_NotDeclaredByInterface));
            error.ShouldContain("The method Base<T> requires that an interface methods to be base-invoked with its return value fluently");
        }
    }
}
