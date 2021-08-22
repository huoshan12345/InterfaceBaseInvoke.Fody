﻿using System;
using System.Collections.Generic;
using System.Text;
using InterfaceBaseInvoke.Tests.AssemblyToProcess;

namespace InterfaceBaseInvoke.Tests.InvalidAssemblyToProcess
{
    public class AnchorMethodTestCases
    {
        private class DerivedHasDefaultMethod : HasDefaultMethod
        {
            public override string Method(int x, string y) => throw new InvalidOperationException();
        }

        public string Base_WithSelf()
        {
            var obj = new HasDefaultMethod();
            return obj.Base().Method(0, string.Empty);
        }

        public string Base_WithParent()
        {
            var obj = new DerivedHasDefaultMethod();
            return obj.Base<HasDefaultMethod>().Method(0, string.Empty);
        }

        public string Base_MoreThanOnce()
        {
            var obj = new HasDefaultMethod();
            return obj.Base<HasDefaultMethod>().Base<HasDefaultMethod>().Method(0, string.Empty);
        }
    }
}
