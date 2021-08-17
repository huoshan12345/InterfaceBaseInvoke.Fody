using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    partial class InvokeOutsideTestCases
    {
        private class InheritIOverridedMethod : IOverridedMethod
        {
            public string Method(int x, string y) => throw new InvalidOperationException();
        }

        public string OverridedMethod_Call()
        {
            var obj = new InheritIOverridedMethod();
            return obj.Base<IOverridedMethod>().Method(2 + (int)Math.Pow(3, 3), $"{nameof(InheritIOverridedMethod)}.{nameof(OverridedMethod_Call)}");
        }

        public string OverridedMethod_MutipleCall()
        {
            var obj = new InheritIOverridedMethod();
            var a = obj.Base<IOverridedMethod>().Method(1, "a");
            var b = obj.Base<IOverridedMethod>().Method(2, "b");
            return a + "----" + b;
        }

        public string OverridedMethod_Call_Jump()
        {
            var obj = new InheritIOverridedMethod();
            var result = "----";
            if (Environment.Is64BitProcess)
            {
                result += obj.Base<IOverridedMethod>().Method(1, "a");
            }
            else
            {
                result += obj.Base<IOverridedMethod>().Method(2, "b");
            }
            result += "----";
            return result;
        }

        public string OverridedMethod_Call_JumpInInvocation()
        {
            var obj = new InheritIOverridedMethod();
            var result = "----";
            if (Environment.Is64BitProcess)
            {
                result += obj.Base<IOverridedMethod>().Method(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 11 : 10, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "aa" : "ab");
            }
            else
            {
                result += obj.Base<IOverridedMethod>().Method(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 21 : 20, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ba" : "bb");
            }
            result += "----";
            return result;
        }
    }
}
