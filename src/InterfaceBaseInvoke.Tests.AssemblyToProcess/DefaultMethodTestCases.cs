using System;

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public class DefaultMethodTestCases
    {
        public StringTestResult Property_Invoke()
        {
            var result = new HasDefaultMethod().Base<IHasDefaultMethod>().Property;
            return ("", result);
        }

        public StringTestResult DefaultMethod_Invoke()
        {
            var obj = new HasDefaultMethod();
            var result = obj.Base<IHasDefaultMethod>().Method(1, "a");
            return ("", result);
        }

        public StringTestResult DefaultMethod_ComplexArguments_Invoke()
        {
            var obj = new HasDefaultMethod();
            var result = obj.Base<IHasDefaultMethod>().Method(1 + (int)Math.Pow(2, 3), nameof(DefaultMethodTestCases) + "." + nameof(DefaultMethod_ComplexArguments_Invoke));
            return ("", result);
        }

        public StringTestResult DefaultMethod_InvokeTwice()
        {
            var obj = new HasDefaultMethod();
            var a = obj.Base<IHasDefaultMethod>().Method(1, "a");
            var b = obj.Base<IHasDefaultMethod>().Method(2, "b");
            var result = a + "----" + b;
            return ("", result);
        }
    }
}
