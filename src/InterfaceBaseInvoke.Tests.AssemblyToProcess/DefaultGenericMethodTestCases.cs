using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public class DefaultGenericMethodTestCases
    {
        private class DefaultGenericMethod : IHasDefaultGenericMethod
        {
            public string Method(int x, string y) => throw new InvalidOperationException();
            public string Method<T>(int x, string y) => throw new InvalidOperationException();
        }

        public string Method_Invoke()
        {
            var obj = new DefaultGenericMethod();
            return obj.Base<IHasDefaultGenericMethod>().Method(2 + (int)Math.Pow(3, 3), $"{nameof(Method_Invoke)}");
        }

        public string Method_InvokeTwice()
        {
            var obj = new DefaultGenericMethod();
            var a = obj.Base<IHasDefaultGenericMethod>().Method(1, "a");
            var b = obj.Base<IHasDefaultGenericMethod>().Method(2, "b");
            return a + "----" + b;
        }

        public string GenericMethod_Invoke()
        {
            var obj = new DefaultGenericMethod();
            return obj.Base<IHasDefaultGenericMethod>().Method<int>(2 + (int)Math.Pow(3, 3), $"{nameof(DefaultGenericMethod)}.{nameof(GenericMethod_Invoke)}");
        }

        public string GenericMethod_InvokeTwice()
        {
            var obj = new DefaultGenericMethod();
            var a = obj.Base<IHasDefaultGenericMethod>().Method<int>(1, "a");
            var b = obj.Base<IHasDefaultGenericMethod>().Method<string>(2, "b");
            return a + "----" + b;
        }
    }
}
