using System.Diagnostics;
using System.Reflection;

namespace InterfaceBaseInvoke.Tests.Support
{
    public class InvalidAssemblyOnlyDebugFactAttribute : FactAttribute
    {
        public override string? Skip
        {
            get => base.Skip ?? (InvalidAssemblyToProcessFixture.IsDebug ? null : "Inconclusive in release builds");
            set => base.Skip = value;
        }
    }
}
