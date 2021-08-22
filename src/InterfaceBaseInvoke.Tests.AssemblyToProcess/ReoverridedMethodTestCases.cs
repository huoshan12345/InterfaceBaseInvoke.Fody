using System;
using System.Diagnostics.CodeAnalysis;

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public class ReoverridedMethodTestCases
    {
        public string Property_InvokeTwice()
        {
            var obj = new HasReoverridedMethod();
            return obj.Base<IHasOverridedMethod>().Property + "----" + obj.Base<IHasReoverridedMethod>().Property;
        }

        public string ReoverrideMethod_InvokeTwice()
        {
            var obj = new HasReoverridedMethod();
            var a = obj.Base<IHasOverridedMethod>().Method(1, "a");
            var b = obj.Base<IHasReoverridedMethod>().Method(2, "b");
            return a + "----" + b;
        }
    }
}
