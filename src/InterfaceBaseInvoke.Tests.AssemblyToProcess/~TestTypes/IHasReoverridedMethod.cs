namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public interface IHasReoverridedMethod : IHasOverridedMethod
    {
        string IHasEmptyMethod.Property => $"{nameof(IHasReoverridedMethod)}.{nameof(Property)}";
        string IHasEmptyMethod.Method(int x, string y) => $"{nameof(IHasReoverridedMethod)}.{nameof(Method)}.{x}.{y}";
    }
}
