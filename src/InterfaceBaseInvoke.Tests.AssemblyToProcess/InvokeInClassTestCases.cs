using System;

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public class InvokeInClassTestCases
    {
        private interface I0
        {
            int Compute(int number) => number;
        }

        private interface I1
        {
            int Compute(int number) => number + 1;
        }

        private class C0 : I0
        {
            public int Compute(int number)
            {
                throw new InvalidOperationException();
            }

            public int InPlace()
            {
                return this.Base<I0>().Compute(2 + (int)Math.Pow(1, 1));
            }

            public int AfterAssignment()
            {
                var obj = this.Base<I0>();
                return obj.Compute(2 + (int)Math.Pow(1, 1));
            }

            public (int, int, int, int) Multiple()
            {
                var obj1 = this.Base<I0>();
                var a = obj1.Compute(2);
                var obj2 = this.Base<I0>();
                var b = obj1.Compute(1 + (int)Math.Pow(2, 2));
                var c = obj2.Compute(2 + (int)Math.Pow(3, 3));
                var d = obj1.Compute(1 + (int)Math.Pow(4, 4)) + obj2.Compute(2 + (int)Math.Pow(4, 4));
                return (a, b, c, d);
            }
        }

        public int InPlace()
        {
            return new C0().InPlace();
        }

        public int AfterAssignment()
        {
            return new C0().AfterAssignment();
        }

        public (int, int, int, int) Multiple()
        {
            return new C0().Multiple();
        }
    }
}
