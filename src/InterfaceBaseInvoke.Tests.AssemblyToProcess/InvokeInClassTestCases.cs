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
            public int Compute(int number) => this.Base<I0, int>(m => m.Compute(number));
        }

        public int Call()
        {
            return new C0().Compute(1);
        }

        public int CallThroughInterface()
        {
            return ((I0)new C0()).Compute(1);
        }
    }
}
