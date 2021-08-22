// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public readonly struct StringTestResult
    {
        public readonly string Expected;
        public readonly string Actual;

        public StringTestResult(string expected, string actual)
        {
            Expected = expected;
            Actual = actual;
        }

        public void Deconstruct(out string expected, out string actual)
        {
            expected = Expected;
            actual = Actual;
        }

        public static implicit operator StringTestResult((string, string) tuple)
        {
            return new(tuple.Item1, tuple.Item2);
        }

        public static implicit operator (string, string)(StringTestResult result)
        {
            return (result.Expected, result.Actual);
        }
    }
}
