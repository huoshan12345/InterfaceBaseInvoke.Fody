namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public interface IOverridedMethod : IEmptyMethod
    {
        string IEmptyMethod.Method(int x, string y) => $"{nameof(IOverridedMethod)}.{nameof(Method)}.{x}.{y}";
    }
}
