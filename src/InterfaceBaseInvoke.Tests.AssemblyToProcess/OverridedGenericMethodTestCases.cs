using System;
using System.Diagnostics.CodeAnalysis;

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public class OverridedGenericMethodTestCases
    {
        private class HasOverridedGenericMethod : IHasOverridedGenericMethod
        {
            public string Method(int x, string y) => throw new InvalidOperationException();
            public string Method<T>(int x, string y) => throw new InvalidOperationException();
        }

        public string Method_Invoke()
        {
            var obj = new HasOverridedGenericMethod();
            return obj.Base<IHasOverridedGenericMethod>().Method(2 + (int)Math.Pow(3, 3), $"{nameof(HasOverridedGenericMethod)}.{nameof(Method_Invoke)}");
        }

        public string Method_InvokeTwice()
        {
            var obj = new HasOverridedGenericMethod();
            var a = obj.Base<IHasOverridedGenericMethod>().Method(1, "a");
            var b = obj.Base<IHasOverridedGenericMethod>().Method(2, "b");
            return a + "----" + b;
        }

        public string GenericMethod_Invoke()
        {
            var obj = new HasOverridedGenericMethod();
            return obj.Base<IHasOverridedGenericMethod>().Method<int>(2 + (int)Math.Pow(3, 3), $"{nameof(HasOverridedGenericMethod)}.{nameof(GenericMethod_Invoke)}");
        }

        public string GenericMethod_InvokeTwice()
        {
            var obj = new HasOverridedGenericMethod();
            var a = obj.Base<IHasOverridedGenericMethod>().Method<int>(1, "a");
            var b = obj.Base<IHasOverridedGenericMethod>().Method<string>(2, "b");
            return a + "----" + b;
        }
    }
}
