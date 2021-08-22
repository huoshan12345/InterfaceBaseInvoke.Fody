namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public class OverridedGenericMethodTestCases
    {
        public StringTestResult Method_Invoke()
        {
            var obj = new HasOverridedGenericMethod();
            var result = obj.Base<IHasOverridedGenericMethod>().Method(1, "a");
            return ("", result);
        }

        public StringTestResult Method_InvokeTwice()
        {
            var obj = new HasOverridedGenericMethod();
            var a = obj.Base<IHasOverridedGenericMethod>().Method(1, "a");
            var b = obj.Base<IHasOverridedGenericMethod>().Method(2, "b");
            var result = a + "----" + b;
            return ("", result);
        }

        public StringTestResult GenericMethod_Invoke()
        {
            var obj = new HasOverridedGenericMethod();
            var result = obj.Base<IHasOverridedGenericMethod>().Method<int>(2, "b");
            return ("", result);
        }

        public StringTestResult GenericMethod_InvokeTwice()
        {
            var obj = new HasOverridedGenericMethod();
            var a = obj.Base<IHasOverridedGenericMethod>().Method<int>(1, "a");
            var b = obj.Base<IHasOverridedGenericMethod>().Method<string>(2, "b");
            var result = a + "----" + b;
            return ("", result);
        }
    }
}
