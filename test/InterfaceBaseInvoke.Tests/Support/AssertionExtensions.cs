using System.Diagnostics;

namespace InterfaceBaseInvoke.Tests.Support;

internal static class AssertionExtensions
{
    [DebuggerStepThrough]
    public static T ShouldNotBeNull<T>(this T? actual) where T : class
    {
        Assert.NotNull(actual);
        return actual;
    }

    [DebuggerStepThrough]
    public static void ShouldNotContain<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        => Assert.DoesNotContain(items, item => predicate(item));

    [DebuggerStepThrough]
    public static void ShouldNotContain(this string str, string expectedSubstring)
        => Assert.DoesNotContain(expectedSubstring, str);

    [DebuggerStepThrough]
    public static void ShouldContain(this string str, string expectedSubstring)
        => Assert.Contains(expectedSubstring, str);
}
