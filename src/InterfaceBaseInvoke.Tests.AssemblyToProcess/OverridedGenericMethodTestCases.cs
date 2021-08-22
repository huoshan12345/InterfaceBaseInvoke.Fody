// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public class OverridedGenericMethodTestCases
    {
        public StringTestResult Method_Invoke()
        {
            var obj = new HasOverridedGenericMethod();
            var result = obj.Base<IHasOverridedGenericMethod>().Method(1, "a");
            return ("Method(1, a)", result);
        }

        public StringTestResult Method_InvokeTwice()
        {
            var obj = new HasOverridedGenericMethod();
            var a = obj.Base<IHasOverridedGenericMethod>().Method(1, "a");
            var b = obj.Base<IHasOverridedGenericMethod>().Method(2, "b");
            var result = a + "----" + b;
            return ("Method(1, a)----Method(2, b)", result);
        }

        public StringTestResult GenericMethod_Invoke()
        {
            var obj = new HasOverridedGenericMethod();
            var result = obj.Base<IHasOverridedGenericMethod>().Method<int>(2, "b");
            return ("Method<Int32>(2, b)", result);
        }

        public StringTestResult GenericMethod_InvokeTwice()
        {
            var obj = new HasOverridedGenericMethod();
            var a = obj.Base<IHasOverridedGenericMethod>().Method<int>(1, "a");
            var b = obj.Base<IHasOverridedGenericMethod>().Method<string>(2, "b");
            var result = a + "----" + b;
            return ("Method<Int32>(1, a)----Method<String>(2, b)", result);
        }
    }
}
