using System;
using System.Diagnostics.CodeAnalysis;

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public class OverridedMethodTestCases
    {
        public string Property_Invoke()
        {
            var obj = new HasOverridedMethod();
            return obj.Base<IHasOverridedMethod>().Property;
        }

        public string OverridedMethod_Invoke()
        {
            var obj = new HasOverridedMethod();
            return obj.Base<IHasOverridedMethod>().Method(2 + (int)Math.Pow(3, 3), $"{nameof(HasOverridedMethod)}.{nameof(OverridedMethod_Invoke)}");
        }

        public string OverridedMethod_InvokeTwice()
        {
            var obj = new HasOverridedMethod();
            var a = obj.Base<IHasOverridedMethod>().Method(1, "a");
            var b = obj.Base<IHasOverridedMethod>().Method(2, "b");
            return a + "----" + b;
        }
    }
}
