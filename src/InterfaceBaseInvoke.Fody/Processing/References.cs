namespace InterfaceBaseInvoke.Fody.Processing
{
    public sealed class References
    {
        public TypeReferences Types { get; }
        public MethodReferences Methods { get; }

        public References(ModuleDefinition module)
        {
            Types = new TypeReferences(module);
            Methods = new MethodReferences(module, Types);
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

        public MethodReferences(ModuleDefinition module, TypeReferences types)
        {
            FunctionPointer = MethodRefBuilder.MethodByNameAndSignature(module, types.RuntimeMethodHandle, nameof(RuntimeMethodHandle.GetFunctionPointer), 0, types.IntPtr, Enumerable.Empty<TypeReference>()).Build();
            ToInt32 = MethodRefBuilder.MethodByName(module, types.IntPtr, nameof(IntPtr.ToInt32)).Build();
            ToInt64 = MethodRefBuilder.MethodByName(module, types.IntPtr, nameof(IntPtr.ToInt64)).Build();
            Is64BitProcess = MethodRefBuilder.PropertyGet(module, types.Environment, nameof(Environment.Is64BitProcess)).Build();
        }
    }
}
