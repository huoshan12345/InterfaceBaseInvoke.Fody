using System;
using System.Diagnostics.CodeAnalysis;

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public class DefaultMethodTestCases
    {
        public static string Property_Invoke()
        {
            return new HasDefaultMethod().Base<IHasDefaultMethod>().Property;
        }

        public static string DefaultMethod_Invoke()
        {
            var obj = new HasDefaultMethod();
            return obj.Base<IHasDefaultMethod>().Method(2 + (int)Math.Pow(3, 3), nameof(HasDefaultMethod));
        }

        public static string DefaultMethod_InvokeTwice()
        {
            var obj = new HasDefaultMethod();
            var a = obj.Base<IHasDefaultMethod>().Method(1, "a");
            var b = obj.Base<IHasDefaultMethod>().Method(2, "b");
            return a + "----" + b;
        }
    }
}
