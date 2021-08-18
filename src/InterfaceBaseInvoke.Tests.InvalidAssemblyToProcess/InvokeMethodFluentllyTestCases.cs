using System;
using System.Collections.Generic;
using System.Text;
using InterfaceBaseInvoke.Tests.AssemblyToProcess;

namespace InterfaceBaseInvoke.Tests.InvalidAssemblyToProcess
{
    public class InvokeMethodFluentllyTestCases
    {
        private class InheritIDefaultMethod : IDefaultMethod
        {
            public string Method(int x, string y) => throw new InvalidOperationException();
        }

        public string InvokeMethod_WithVariable()
        {
            var obj = new InheritIDefaultMethod().Base<IDefaultMethod>();
            return obj.Method(0, string.Empty);
        }

        public string InvokeMethod_NotDeclaredByInterface()
        {
            var obj = new InheritIDefaultMethod();
            return obj.Base<IDefaultMethod>().ToString();
        }
    }
}
