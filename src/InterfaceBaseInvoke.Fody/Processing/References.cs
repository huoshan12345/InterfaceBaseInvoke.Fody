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

public sealed class TypeReferences(ModuleWeavingContext Context)
{
    public TypeReference RuntimeMethodHandle { get; } = Context.ImportReference(typeof(RuntimeMethodHandle));
    public TypeReference IntPtr { get; } = Context.ImportReference(typeof(IntPtr));
}

public sealed class MethodReferences(ModuleWeavingContext Context, TypeReferences Types)
{
    public MethodReference FunctionPointer { get; } = MethodRefBuilder.MethodByNameAndSignature(Context, Types.RuntimeMethodHandle, nameof(RuntimeMethodHandle.GetFunctionPointer), 0, Types.IntPtr.ToTypeRefBuilder(Context), []).Build();
}
