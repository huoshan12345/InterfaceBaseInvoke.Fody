using System;
using System.Diagnostics.CodeAnalysis;
using InterfaceBaseInvoke.Tests.AssemblyToProcess;

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class InvokeOutsideTestCases
    {
        private class InheritIDefaultMethod : IDefaultMethod
        {
            public string Method(int x, string y) => throw new InvalidOperationException();
        }

        public string DefaultMethod_Call()
        {
            var obj = new InheritIDefaultMethod();
            return obj.Base<IDefaultMethod>().Method(2 + (int)Math.Pow(3, 3), $"{nameof(InheritIDefaultMethod)}.{nameof(DefaultMethod_Call)}");
        }

        public string DefaultMethod_MutipleCall()
        {
            var obj = new InheritIDefaultMethod();
            var a = obj.Base<IDefaultMethod>().Method(1, "a");
            var b = obj.Base<IDefaultMethod>().Method(2, "b");
            return a + "----" + b;
        }
    }
}
