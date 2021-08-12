namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public class InvokeInClassTestCases
    {
        private interface I0
        {
            int Compute(int number) => number + 1;
        }

        private class C0 : I0
        {
            public int Compute(int number)
            {
                var obj = this.Base<I0>();
                var a = obj.Compute(number + 1);
                var b = obj.Compute(number + 1);
                return a + b;
            }
        }

        public int Call()
        {
            return new C0().Compute(1);
        }
    }
}
