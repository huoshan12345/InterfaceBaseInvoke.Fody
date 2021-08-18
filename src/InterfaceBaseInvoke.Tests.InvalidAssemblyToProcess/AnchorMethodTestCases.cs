using System;
using System.Collections.Generic;
using System.Text;
using InterfaceBaseInvoke.Tests.AssemblyToProcess;

namespace InterfaceBaseInvoke.Tests.InvalidAssemblyToProcess
{
    public class AnchorMethodTestCases
    {
        private class InheritIEmptyMethod : IEmptyMethod
        {
            public virtual string Method(int x, string y) => "";
        }

        private class DerivedInheritIEmptyMethod : InheritIEmptyMethod
        {
            public override string Method(int x, string y) => "";
        }

        public string Base_WithSelf()
        {
            var obj = new InheritIEmptyMethod();
            return obj.Base().Method(0, string.Empty);
        }

        public string Base_WithParent()
        {
            var obj = new InheritIEmptyMethod();
            return obj.Base().Method(0, string.Empty);
        }

        public string Base_MoreThanOnce()
        {
            var obj = new InheritIEmptyMethod();
            return obj.Base<IEmptyMethod>().Base<IEmptyMethod>().Method(0, string.Empty);
        }
    }
}
