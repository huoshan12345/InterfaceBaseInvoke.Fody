using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public class DefaultGenericMethodTestCases
    {
        public string Method_Invoke()
        {
            var obj = new HasDefaultGenericMethod();
            return obj.Base<IHasDefaultGenericMethod>().Method(2 + (int)Math.Pow(3, 3), $"{nameof(IHasDefaultGenericMethod)}");
        }

        public string Method_InvokeTwice()
        {
            var obj = new HasDefaultGenericMethod();
            var a = obj.Base<IHasDefaultGenericMethod>().Method(1, "a");
            var b = obj.Base<IHasDefaultGenericMethod>().Method(2, "b");
            return a + "----" + b;
        }

        public string GenericMethod_Invoke()
        {
            var obj = new HasDefaultGenericMethod();
            return obj.Base<IHasDefaultGenericMethod>().Method<int>(2 + (int)Math.Pow(3, 3), nameof(IHasDefaultGenericMethod));
        }

        public string GenericMethod_InvokeTwice()
        {
            var obj = new HasDefaultGenericMethod();
            var a = obj.Base<IHasDefaultGenericMethod>().Method<int>(1, "a");
            var b = obj.Base<IHasDefaultGenericMethod>().Method<string>(2, "b");
            return a + "----" + b;
        }
    }
}
