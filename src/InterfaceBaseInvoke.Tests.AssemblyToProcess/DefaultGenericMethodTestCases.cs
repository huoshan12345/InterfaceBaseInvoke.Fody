// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public class DefaultGenericMethodTestCases
    {
        public StringTestResult Method_Invoke()
        {
            var obj = new HasDefaultGenericMethod();
            var result = obj.Base<IHasDefaultGenericMethod>().Method(1, "a");
            return ("Method(1, a)", result);
        }

        public StringTestResult Method_InvokeTwice()
        {
            var obj = new HasDefaultGenericMethod();
            var a = obj.Base<IHasDefaultGenericMethod>().Method(1, "a");
            var b = obj.Base<IHasDefaultGenericMethod>().Method(2, "b");
            var result = a + "----" + b;
            return ("Method(1, a)----Method(2, b)", result);
        }

        public StringTestResult GenericMethod_Invoke()
        {
            var obj = new HasDefaultGenericMethod();
            var result = obj.Base<IHasDefaultGenericMethod>().Method<int>(2, "b");
            return ("Method<Int32>(2, b)", result);
        }

        public StringTestResult GenericMethod_InvokeTwice()
        {
            var obj = new HasDefaultGenericMethod();
            var a = obj.Base<IHasDefaultGenericMethod>().Method<int>(1, "a");
            var b = obj.Base<IHasDefaultGenericMethod>().Method<string>(2, "b");
            var result = a + "----" + b;
            return ("Method<Int32>(1, a)----Method<String>(2, b)", result);
        }
    }
}
