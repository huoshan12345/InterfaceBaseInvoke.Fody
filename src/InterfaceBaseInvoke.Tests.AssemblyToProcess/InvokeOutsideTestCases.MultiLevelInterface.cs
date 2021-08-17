﻿using System;

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    partial class InvokeOutsideTestCases
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
    }
}
