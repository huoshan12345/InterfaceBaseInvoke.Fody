namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public interface IHasDefaultMethod
    {
        string Property => $"{nameof(IHasDefaultMethod)}.{nameof(Property)}";
        string Method(int x, string y) => $"{nameof(IHasDefaultMethod)}.{nameof(Method)}.{x}.{y}";
    }
}
