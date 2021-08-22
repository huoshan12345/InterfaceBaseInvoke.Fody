using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public class GenericInterfaceTestCases
    {
        public string Method_Invoke()
        {
            var obj = new GenericInterface<int>();
            return obj.Base<IGenericHasDefaultGenericMethod<int>>().Method(1, "a");
        }

        public string Method_InvokeTwice()
        {
            var obj = new GenericInterface<string>();
            var a = obj.Base<IGenericHasDefaultGenericMethod<string>>().Method(1, "a");
            var b = obj.Base<IGenericHasDefaultGenericMethod<string>>().Method(2, "b");
            return a + "----" + b;
        }

        public string GenericMethod_Invoke()
        {
            var obj = new GenericInterface<int>();
            return obj.Base<IGenericHasDefaultGenericMethod<int>>().Method<string>(1, "a");
        }

        public string GenericMethod_InvokeTwice()
        {
            var obj = new GenericInterface<string>();
            var a = obj.Base<IGenericHasDefaultGenericMethod<string>>().Method<string>(1, "a");
            var b = obj.Base<IGenericHasDefaultGenericMethod<string>>().Method<int>(2, "b");
            return a + "----" + b;
        }
    }
}
