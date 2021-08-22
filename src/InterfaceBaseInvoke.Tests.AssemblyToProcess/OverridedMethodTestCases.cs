namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public class OverridedMethodTestCases
    {
        public StringTestResult Property_Invoke()
        {
            var obj = new HasOverridedMethod();
            var result = obj.Base<IHasOverridedMethod>().Property;
            return ("", result);
        }

        public StringTestResult OverridedMethod_Invoke()
        {
            var obj = new HasOverridedMethod();
            var result = obj.Base<IHasOverridedMethod>().Method(1, "a");
            return ("", result);
        }

        public StringTestResult OverridedMethod_InvokeTwice()
        {
            var obj = new HasOverridedMethod();
            var a = obj.Base<IHasOverridedMethod>().Method(1, "a");
            var b = obj.Base<IHasOverridedMethod>().Method(2, "b");
            var result = a + "----" + b;
            return ("", result);
        }
    }
}
