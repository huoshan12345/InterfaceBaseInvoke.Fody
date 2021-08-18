using System;
using System.Collections.Generic;
using System.Text;
using InterfaceBaseInvoke.Tests.AssemblyToProcess;

namespace InterfaceBaseInvoke.Tests.InvalidAssemblyToProcess
{
    public class InvokeAbstractMethodTestCases
    {
        private class InheritIEmptyMethod : IEmptyMethod
        {
            public string Method(int x, string y) => throw new InvalidOperationException();
        }

        private class InheritIOverridedMethod : IOverridedMethod
        {
            public string Method(int x, string y) => throw new InvalidOperationException();
        }

        public string EmptyMethod_Call()
        {
            var obj = new InheritIEmptyMethod();
            return obj.Base<IEmptyMethod>().Method(0, string.Empty);
        }

        public string EmptyMethod_Call_MultiLevel()
        {
            var obj = new InheritIOverridedMethod();
            return obj.Base<IEmptyMethod>().Method(0, string.Empty);
        }
    }
}
