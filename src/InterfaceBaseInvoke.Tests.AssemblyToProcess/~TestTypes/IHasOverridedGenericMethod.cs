namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public interface IHasOverridedGenericMethod : IHasEmptyGenericMethod
    {
        string IHasEmptyGenericMethod.Method(int x, string y) => $"{nameof(IHasOverridedGenericMethod)}.{nameof(Method)}.{x}.{y}";
        string IHasEmptyGenericMethod.Method<T>(int x, string y) => $"{nameof(IHasOverridedGenericMethod)}.{nameof(Method)}.{x}.{y}.{typeof(T).Name}";
    }
}
