﻿using InterfaceBaseInvoke.Tests.AssemblyToProcess;

namespace InterfaceBaseInvoke.Tests.InvalidAssemblyToProcess
{
    public class InvokeAbstractMethodTestCases
    {
        private class InheritIEmptyMethod : IHasEmptyMethod
        {
            public string Property => throw new InvalidOperationException();
            public string Method(int x, string y) => throw new InvalidOperationException();
        }

        private class InheritIOverrideMethod : IHasOverrideMethod
        {
            public string Method(int x, string y) => throw new InvalidOperationException();
        }

        public string EmptyMethod_Invoke()
        {
            var obj = new InheritIEmptyMethod();
            return obj.Base<IHasEmptyMethod>().Method(0, string.Empty);
        }

        public string EmptyMethod_Invoke_MultiLevel()
        {
            var obj = new InheritIOverrideMethod();
            return obj.Base<IHasEmptyMethod>().Method(0, string.Empty);
        }
    }
}
