using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fody;
using InterfaceBaseInvoke.Fody.Extensions;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace InterfaceBaseInvoke.Fody.Models
{
    internal class MethodRefBuilder
    {
        private readonly ModuleDefinition _module;
        private MethodReference _method;

        private MethodRefBuilder(ModuleDefinition module, TypeReference typeRef, MethodReference method)
        {
            _module = module;

            method = method.MapToScope(typeRef.Scope, module);
            _method = _module.ImportReference(_module.ImportReference(method).MakeGeneric(typeRef));
        }

        public static MethodRefBuilder MethodByName(ModuleDefinition module, TypeReference typeRef, string methodName)
            => new(module, typeRef, FindMethod(typeRef, methodName, null, null, null));

        public static MethodRefBuilder MethodByNameAndSignature(ModuleDefinition module, TypeReference typeRef, string methodName, int? genericArity, TypeReference? returnType, IReadOnlyList<TypeReference> paramTypes)
            => new(module, typeRef, FindMethod(typeRef, methodName, genericArity, returnType, paramTypes ?? throw new ArgumentNullException(nameof(paramTypes))));
        
        private static MethodReference FindMethod(TypeReference typeRef, string methodName, int? genericArity, TypeReference? returnType, IReadOnlyList<TypeReference>? paramTypes)
        {
            var typeDef = typeRef.ResolveRequiredType();

            var methods = typeDef.Methods.Where(m => m.Name == methodName);

            if (genericArity != null)
            {
                methods = genericArity == 0
                    ? methods.Where(m => !m.HasGenericParameters)
                    : methods.Where(m => m.HasGenericParameters && m.GenericParameters.Count == genericArity);
            }

            if (returnType != null)
                methods = methods.Where(m => m.ReturnType.FullName == returnType.FullName);

            if (paramTypes != null)
                methods = methods.Where(m => SignatureMatches(m, paramTypes));

            var methodList = methods.ToList();

            return methodList.Count switch
            {
                1 => methodList.Single(),
                0 => throw new WeavingException($"Method {GetDisplaySignature(methodName, genericArity, returnType, paramTypes)} not found in type {typeDef.FullName}"),
                _ => throw new WeavingException($"Ambiguous method {GetDisplaySignature(methodName, genericArity, returnType, paramTypes)} in type {typeDef.FullName}")
            };
        }

        private static bool SignatureMatches(MethodReference method, IReadOnlyList<TypeReference> paramTypes)
        {
            if (method.Parameters.Count != paramTypes.Count)
                return false;

            for (var i = 0; i < paramTypes.Count; ++i)
            {
                var paramType = paramTypes[i];
                if (paramType == null)
                    return false;

                if (method.Parameters[i].ParameterType.FullName != paramType.FullName)
                    return false;
            }

            return true;
        }

        private static string GetDisplaySignature(string methodName, int? genericArity, TypeReference? returnType, IReadOnlyList<TypeReference>? paramTypes)
        {
            if (genericArity is null && returnType is null && paramTypes is null)
                return "'" + methodName + "'";

            var sb = new StringBuilder();

            if (returnType != null)
                sb.Append(returnType.FullName).Append(' ');

            sb.Append(methodName);

            switch (genericArity)
            {
                case 0:
                case null:
                    break;

                case 1:
                    sb.Append("<T>");
                    break;

                default:
                    sb.Append('<');

                    for (var i = 0; i < genericArity.GetValueOrDefault(); ++i)
                    {
                        if (i != 0)
                            sb.Append(", ");

                        sb.Append('T').Append(i);
                    }

                    sb.Append('>');
                    break;
            }

            if (paramTypes != null)
            {
                sb.Append('(');

                for (var i = 0; i < paramTypes.Count; ++i)
                {
                    if (i != 0)
                        sb.Append(", ");

                    sb.Append(paramTypes[i].FullName);
                }

                sb.Append(')');
            }

            return sb.ToString();
        }

        public static MethodRefBuilder PropertyGet(ModuleDefinition module, TypeReference typeRef, string propertyName)
        {
            var property = FindProperty(typeRef, propertyName);

            if (property.GetMethod == null)
                throw new WeavingException($"Property '{propertyName}' in type {typeRef.FullName} has no getter");

            return new MethodRefBuilder(module, typeRef, property.GetMethod);
        }

        public static MethodRefBuilder PropertySet(ModuleDefinition module, TypeReference typeRef, string propertyName)
        {
            var property = FindProperty(typeRef, propertyName);

            if (property.SetMethod == null)
                throw new WeavingException($"Property '{propertyName}' in type {typeRef.FullName} has no setter");

            return new MethodRefBuilder(module, typeRef, property.SetMethod);
        }

        private static PropertyDefinition FindProperty(TypeReference typeRef, string propertyName)
        {
            var typeDef = typeRef.ResolveRequiredType();

            var properties = typeDef.Properties.Where(p => p.Name == propertyName).ToList();

            return properties.Count switch
            {
                1 => properties.Single(),
                0 => throw new WeavingException($"Property '{propertyName}' not found in type {typeDef.FullName}"),
                _ => throw new WeavingException($"Ambiguous property '{propertyName}' in type {typeDef.FullName}")
            };
        }

        public MethodReference Build()
            => _method;

        public void MakeGenericMethod(TypeReference[] genericArgs)
        {
            if (!_method.HasGenericParameters)
                throw new WeavingException($"Not a generic method: {_method.FullName}");

            if (genericArgs.Length == 0)
                throw new WeavingException("No generic arguments supplied");

            if (_method.GenericParameters.Count != genericArgs.Length)
                throw new WeavingException($"Incorrect number of generic arguments supplied for method {_method.FullName} - expected {_method.GenericParameters.Count}, but got {genericArgs.Length}");

            var genericInstance = new GenericInstanceMethod(_method);
            genericInstance.GenericArguments.AddRange(genericArgs);

            _method = _module.ImportReference(genericInstance);
        }

        public void SetOptionalParameters(TypeReference[] optionalParamTypes)
        {
            if (_method.CallingConvention != MethodCallingConvention.VarArg)
                throw new WeavingException($"Not a vararg method: {_method.FullName}");

            if (_method.Parameters.Any(p => p.ParameterType.IsSentinel))
                throw new WeavingException("Optional parameters for vararg call have already been supplied");

            if (optionalParamTypes.Length == 0)
                throw new WeavingException("No optional parameter type supplied for vararg method call");

            var methodRef = _method.Clone();
            methodRef.CallingConvention = MethodCallingConvention.VarArg;

            for (var i = 0; i < optionalParamTypes.Length; ++i)
            {
                var paramType = optionalParamTypes[i];
                if (i == 0)
                    paramType = paramType.MakeSentinelType();

                methodRef.Parameters.Add(new ParameterDefinition(paramType));
            }

            _method = _module.ImportReference(methodRef);
        }

        public override string ToString() => _method.ToString();
    }
}
