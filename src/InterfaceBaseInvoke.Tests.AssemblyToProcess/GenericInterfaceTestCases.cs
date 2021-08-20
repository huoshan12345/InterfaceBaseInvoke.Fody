using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public class GenericInterfaceTestCases
    {
        private class GenericClass<T> : IGenericInterface<T>
        {
            public string Method(int x, string y) => throw new InvalidOperationException();
            public string Method<TParameter>(int x, string y) => throw new InvalidOperationException();
        }

        public string Method_Invoke()
        {
            var obj = new GenericClass<int>();
            return obj.Base<IGenericInterface<int>>().Method(2 + (int)Math.Pow(3, 3), $"{nameof(GenericClass<int>)}.{nameof(Method_Invoke)}");
        }

        public string Method_InvokeTwice()
        {
            var obj = new GenericClass<string>();
            var a = obj.Base<IGenericInterface<string>>().Method(1, "a");
            var b = obj.Base<IGenericInterface<string>>().Method(2, "b");
            return a + "----" + b;
        }

        public string GenericMethod_Invoke()
        {
            var obj = new GenericClass<int>();
            return obj.Base<IGenericInterface<int>>().Method<string>(2 + (int)Math.Pow(3, 3), $"{nameof(GenericClass<string>)}.{nameof(GenericMethod_Invoke)}");
        }

        public string GenericMethod_InvokeTwice()
        {
            var obj = new GenericClass<string>();
            var a = obj.Base<IGenericInterface<string>>().Method<string>(1, "a");
            var b = obj.Base<IGenericInterface<string>>().Method<int>(2, "b");
            return a + "----" + b;
        }
    }
}
