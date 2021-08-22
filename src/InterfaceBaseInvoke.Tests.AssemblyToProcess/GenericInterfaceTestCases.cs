// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public class GenericInterfaceTestCases
    {
        public StringTestResult Method_Invoke()
        {
            var obj = new GenericInterface<int>();
            var result = obj.Base<IGenericHasDefaultGenericMethod<int>>().Method(1, "a");
            return ("Method(1, a)", result);
        }

        public StringTestResult Method_InvokeTwice()
        {
            var obj = new GenericInterface<string>();
            var a = obj.Base<IGenericHasDefaultGenericMethod<string>>().Method(1, "a");
            var b = obj.Base<IGenericHasDefaultGenericMethod<string>>().Method(2, "b");
            var result = a + "----" + b;
            return ("Method(1, a)----Method(2, b)", result);
        }

        public StringTestResult GenericMethod_Invoke()
        {
            var obj = new GenericInterface<int>();
            var result = obj.Base<IGenericHasDefaultGenericMethod<int>>().Method<string>(1, "a");
            return ("Method<String>(1, a)", result);
        }

        public StringTestResult GenericMethod_InvokeTwice()
        {
            var obj = new GenericInterface<string>();
            var a = obj.Base<IGenericHasDefaultGenericMethod<string>>().Method<string>(1, "a");
            var b = obj.Base<IGenericHasDefaultGenericMethod<string>>().Method<int>(2, "b");
            var result = a + "----" + b;
            return ("Method<String>(1, a)----Method<Int32>(2, b)", result);
        }
    }
}
