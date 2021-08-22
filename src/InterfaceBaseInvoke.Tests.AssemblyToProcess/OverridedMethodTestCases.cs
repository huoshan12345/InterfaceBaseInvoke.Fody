// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public class OverridedMethodTestCases
    {
        public StringTestResult Property_Invoke()
        {
            var obj = new HasOverridedMethod();
            var result = obj.Base<IHasOverridedMethod>().Property;
            return ("Property", result);
        }

        public StringTestResult OverridedMethod_Invoke()
        {
            var obj = new HasOverridedMethod();
            var result = obj.Base<IHasOverridedMethod>().Method(1, "a");
            return ("Method(1, a)", result);
        }

        public StringTestResult OverridedMethod_InvokeTwice()
        {
            var obj = new HasOverridedMethod();
            var a = obj.Base<IHasOverridedMethod>().Method(1, "a");
            var b = obj.Base<IHasOverridedMethod>().Method(2, "b");
            var result = a + "----" + b;
            return ("Method(1, a)----Method(2, b)", result);
        }
    }
}
