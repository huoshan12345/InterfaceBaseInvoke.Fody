using System;
using System.Linq.Expressions;

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public class InvokeOutsideTestCases
    {
        private interface I0
        {
            int Compute(int number) => number + 1;
        }

        private class C0 : I0
        {
        }

        public int Call0()
        {
            I0 c = new C0();
            return c.Compute(2 + (int)Math.Pow(1, 1));
        }

        public int Call()
        {
            var c = new C0();
            return c.Base<I0>().Compute(2 + (int)Math.Pow(1, 1));
        }
    }
}
