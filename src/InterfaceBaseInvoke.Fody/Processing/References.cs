namespace InterfaceBaseInvoke.Fody.Processing;

public sealed class References
{
    public TypeReferences Types { get; }
    public MethodReferences Methods { get; }

    public References(ModuleWeavingContext context)
    {
        Types = new TypeReferences(context);
        Methods = new MethodReferences(context, Types);
    }
}

public sealed class TypeReferences(ModuleWeavingContext context)
{
    public TypeReference RuntimeMethodHandle { get; } = context.ImportReference(typeof(RuntimeMethodHandle));
    public TypeReference IntPtr { get; } = context.ImportReference(typeof(IntPtr));
}

public sealed class MethodReferences(ModuleWeavingContext context, TypeReferences types)
{
    public MethodReference FunctionPointer { get; } = MethodRefBuilder.MethodByNameAndSignature(context, types.RuntimeMethodHandle, nameof(RuntimeMethodHandle.GetFunctionPointer), 0, types.IntPtr.ToTypeRefBuilder(context), []).Build();
}
