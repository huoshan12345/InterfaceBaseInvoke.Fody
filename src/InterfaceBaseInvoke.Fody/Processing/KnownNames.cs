namespace InterfaceBaseInvoke.Fody.Processing
{
    internal static class KnownNames
    {
        public static class Short
        {
            public const string TypeRefType = "TypeRef";
            public const string MethodRefType = "MethodRef";
            public const string FieldRefType = "FieldRef";
            public const string StandAloneMethodSigType = "StandAloneMethodSig";

            public const string PushMethod = "Push";
            public const string PushInRefMethod = "PushInRef";
            public const string PushOutRefMethod = "PushOutRef";
            public const string PopMethod = "Pop";
            public const string UnreachableMethod = "Unreachable";
            public const string ReturnMethod = "Return";
            public const string ReturnRefMethod = "ReturnRef";
            public const string ReturnPointerMethod = "ReturnPointer";
            public const string MarkLabelMethod = "MarkLabel";
            public const string DeclareLocalsMethod = "DeclareLocals";
            public const string EnsureLocalMethod = "EnsureLocal";
        }
    }
}
