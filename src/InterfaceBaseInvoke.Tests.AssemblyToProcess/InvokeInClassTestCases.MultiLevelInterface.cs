using System;

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    partial class InvokeInClassTestCases
    {
        private class InheritIOverridedMethod : IOverridedMethod
        {
            public string Method(int x, string y)
            {
                throw new InvalidOperationException();
            }

            public string Call()
            {
                return this.Base<IOverridedMethod>().Method(2 + (int)Math.Pow(3, 3), $"{nameof(InheritIOverridedMethod)}.{nameof(Call)}");
            }

            public string MutipleCall()
            {
                var a = this.Base<IOverridedMethod>().Method(1, "a");
                var b = this.Base<IOverridedMethod>().Method(2, "b");
                return a + "----" + b;
            }
        }

        public string OverridedMethod_Call()
        {
            return new InheritIOverridedMethod().Call();
        }

        public string OverridedMethod_MutipleCall()
        {
            return new InheritIOverridedMethod().MutipleCall();
        }
    }
}
