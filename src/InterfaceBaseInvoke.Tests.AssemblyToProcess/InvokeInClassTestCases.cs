using System;
using System.Diagnostics.CodeAnalysis;
using InterfaceBaseInvoke.Tests.AssemblyToProcess;

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class InvokeInClassTestCases
    {
        private class InheritIDefaultMethod : IDefaultMethod
        {
            public string Method(int x, string y)
            {
                throw new InvalidOperationException();
            }

            public string Call()
            {
                return this.Base<IDefaultMethod>().Method(2 + (int)Math.Pow(3, 3), $"{nameof(InheritIDefaultMethod)}.{nameof(Call)}");
            }

            public string MutipleCall()
            {
                var a = this.Base<IDefaultMethod>().Method(1, "a");
                var b = this.Base<IDefaultMethod>().Method(2, "b");
                return a + "----" + b;
            }
        }

        public string DefaultMethod_Call()
        {
            return new InheritIDefaultMethod().Call();
        }

        public string DefaultMethod_MutipleCall()
        {
            return new InheritIDefaultMethod().MutipleCall();
        }
    }
}
