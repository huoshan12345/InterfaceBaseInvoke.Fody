namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public interface IDefaultMethod
    {
        string Method(int x, string y) => $"{nameof(IDefaultMethod)}.{nameof(Method)}.{x}.{y}";
    }
}
