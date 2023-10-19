using System.Reflection;

namespace InterfaceBaseInvoke.Fody.Extensions;

internal static partial class CecilExtensions
{

    public static GenericInstanceMethod MakeGenericMethod(this MethodReference self, IEnumerable<TypeReference> args)
    {
        if (!self.HasGenericParameters)
            throw new WeavingException($"Not a generic method: {self.FullName}");

        var arguments = args.ToArray();

        if (arguments.Length == 0)
            throw new WeavingException("No generic arguments supplied");

        if (self.GenericParameters.Count != arguments.Length)
            throw new ArgumentException($"Incorrect number of generic arguments supplied for method {self.FullName} - expected {self.GenericParameters.Count}, but got {arguments.Length}");

        var instance = new GenericInstanceMethod(self);
        foreach (var argument in arguments)
            instance.GenericArguments.Add(argument);

        return instance;
    }

    public static MethodDefinition GetInterfaceDefaultMethod(this TypeDefinition typeDef, MethodReference methodRef)
    {
        var elementMethod = methodRef.GetElementMethod();
        var methods = typeDef.Methods.Where(m => m.Overrides.Any(x => x.IsEqualTo(elementMethod))).ToList();
        return methods.Count switch
        {
            0   => throw new MissingMethodException(methodRef.Name),
            > 1 => throw new AmbiguousMatchException($"Found more than one method in type {typeDef.Name} by name " + methodRef.Name),
            _   => methods[0]
        };
    }
}
