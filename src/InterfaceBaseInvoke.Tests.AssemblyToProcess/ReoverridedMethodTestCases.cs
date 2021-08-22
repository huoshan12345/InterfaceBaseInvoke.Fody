// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public class ReoverridedMethodTestCases
    {
        public StringTestResult Property_InvokeTwice()
        {
            var obj = new HasReoverridedMethod();
            var result = obj.Base<IHasOverridedMethod>().Property + "----" + obj.Base<IHasReoverridedMethod>().Property;
            return ("Property----IHasReoverridedMethod.Property", result);
        }

        public StringTestResult ReoverrideMethod_InvokeTwice()
        {
            var obj = new HasReoverridedMethod();
            var a = obj.Base<IHasOverridedMethod>().Method(1, "a");
            var b = obj.Base<IHasReoverridedMethod>().Method(2, "b");
            var result = a + "----" + b;
            return ("Method(1, a)----IHasReoverridedMethod.Method(2, b)", result);
        }
    }
}
