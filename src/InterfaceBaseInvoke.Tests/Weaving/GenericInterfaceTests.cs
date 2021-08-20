using InterfaceBaseInvoke.Tests.AssemblyToProcess;
using Xunit;

namespace InterfaceBaseInvoke.Tests.Weaving
{
    public class GenericInterfaceTests : ClassTestsBase
    {
        protected override string ClassName => nameof(GenericInterfaceTestCases);

        [Fact]
        public void Method_Invoke()
        {
            string value = GetInstance().Method_Invoke();
            Assert.Equal("IHasDefaultMethod.Method.29.GenericClass.Method_Invoke.Int32", value);
        }

        [Fact]
        public void Method_InvokeTwice()
        {
            string value = GetInstance().Method_InvokeTwice();
            Assert.Equal("IHasDefaultMethod.Method.1.a.String----IHasDefaultMethod.Method.2.b.String", value);
        }

        [Fact]
        public void GenericMethod_Invoke()
        {
            string value = GetInstance().GenericMethod_Invoke();
            Assert.Equal("IHasDefaultMethod.Method.29.GenericClass.GenericMethod_Invoke.String", value);
        }

        [Fact]
        public void GenericMethod_InvokeTwice()
        {
            string value = GetInstance().GenericMethod_InvokeTwice();
            Assert.Equal("IHasDefaultMethod.Method.1.a.String----IHasDefaultMethod.Method.2.b.Int32", value);
        }
    }

    public class GenericInterfaceTestsStandard : DefaultMethodTests
    {
        protected override bool NetStandard => true;
    }
}
