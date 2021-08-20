namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public interface IHasOverridedMethod : IHasEmptyMethod
    {
        string IHasEmptyMethod.Property => $"{nameof(IHasOverridedMethod)}.{nameof(Property)}";
        string IHasEmptyMethod.Method(int x, string y) => $"{nameof(IHasOverridedMethod)}.{nameof(Method)}.{x}.{y}";
    }
}
