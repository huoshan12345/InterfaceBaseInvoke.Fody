namespace InterfaceBaseInvoke.Fody.Processing;

public sealed class References
{
    public TypeReferences Types { get; }
    public MethodReferences Methods { get; }

    public References(ModuleWeavingContext context)
    {
        Types = new TypeReferences(context.Module);
        Methods = new MethodReferences(context, Types);
    }
}

public sealed class TypeReferences
{
    public TypeReference RuntimeMethodHandle { get; }
    public TypeReference IntPtr { get; }
    public TypeReference Int32 { get; }
    public TypeReference Environment { get; }
    public TypeReference Boolean { get; }

    public TypeReferences(ModuleDefinition module)
    {
        Environment = module.ImportReference(typeof(Environment));
        IntPtr = module.ImportReference(typeof(IntPtr));
        Int32 = module.ImportReference(typeof(int));
        RuntimeMethodHandle = module.ImportReference(typeof(RuntimeMethodHandle));
        Boolean = module.ImportReference(typeof(bool));
    }
}

public sealed class MethodReferences
{
    public MethodReference FunctionPointer { get; }
    public MethodReference ToInt32 { get; }
    public MethodReference ToInt64 { get; }
    public MethodReference Is64BitProcess { get; }

    public MethodReferences(ModuleWeavingContext context, TypeReferences types)
    {
        FunctionPointer = MethodRefBuilder.MethodByNameAndSignature(context, types.RuntimeMethodHandle, nameof(RuntimeMethodHandle.GetFunctionPointer), 0, types.IntPtr.ToTypeRefBuilder(context), []).Build();
        ToInt32 = MethodRefBuilder.MethodByName(context, types.IntPtr, nameof(IntPtr.ToInt32)).Build();
        ToInt64 = MethodRefBuilder.MethodByName(context, types.IntPtr, nameof(IntPtr.ToInt64)).Build();
        Is64BitProcess = MethodRefBuilder.PropertyGet(context, types.Environment, nameof(Environment.Is64BitProcess)).Build();
    }
}
