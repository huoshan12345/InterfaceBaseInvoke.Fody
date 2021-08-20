namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public interface IHasEmptyGenericMethod
    {
        string Method(int x, string y);
        string Method<T>(int x, string y);
    }
}
