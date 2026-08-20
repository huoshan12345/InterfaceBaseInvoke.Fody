using System.Runtime.CompilerServices;

namespace InterfaceBaseInvoke.Tests.Support;

[AttributeUsage(AttributeTargets.Method)]
public class InvalidAssemblyOnlyDebugFactAttribute : FactAttribute
{
    public InvalidAssemblyOnlyDebugFactAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        Skip = (InvalidAssemblyToProcessFixture.IsDebug ? null : "Inconclusive in release builds");
    }
}
