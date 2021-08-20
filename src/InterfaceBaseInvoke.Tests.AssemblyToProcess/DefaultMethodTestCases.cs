using System;
using System.Diagnostics.CodeAnalysis;

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public class DefaultMethodTestCases
    {
        private class HasDefaultMethod : IHasDefaultMethod
        {
            public string Property => throw new InvalidOperationException();
            public string Method(int x, string y) => throw new InvalidOperationException();

            public string Invoke()
            {
                return this.Base<IHasDefaultMethod>().Method(2 + (int)Math.Pow(3, 3), $"{nameof(HasDefaultMethod)}.{nameof(Invoke)}");
            }

            public string InvokeTwice()
            {
                var a = this.Base<IHasDefaultMethod>().Method(1, "a");
                var b = this.Base<IHasDefaultMethod>().Method(2, "b");
                return a + "----" + b;
            }
        }

        public string Property_Invoke()
        {
            return new HasDefaultMethod().Base<IHasDefaultMethod>().Property;
        }

        public string DefaultMethod_Invoke()
        {
            return new HasDefaultMethod().Invoke();
        }

        public string DefaultMethod_InvokeTwice()
        {
            return new HasDefaultMethod().InvokeTwice();
        }
    }
}
