using Mono.Cecil;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace InterfaceBaseInvoke.Fody.Extensions
{
    internal static partial class CecilExtensions
    {
        public const string AssemblyName = "InterfaceBaseInvoke";

        private static readonly ConcurrentDictionary<TypeReference, bool> _usageCache = new();

        public static bool IsWeaverAssemblyReferenced([NotNullWhen(true)] this TypeReference? type, ModuleDefinition module)
        {
            if (type == null)
                return false;

            return _usageCache.GetOrAdd(type, k => DoCheck(k, module));

            static bool DoCheck(TypeReference typeRef, ModuleDefinition module)
            {
                return typeRef switch
                {
                    GenericInstanceType t => t.ElementType.IsWeaverAssemblyReferenced(module)
                                             || t.GenericParameters.Any(i => i.IsWeaverAssemblyReferenced(module))
                                             || t.GenericArguments.Any(i => i.IsWeaverAssemblyReferenced(module)),
                    GenericParameter t    => t.HasConstraints && t.Constraints.Any(c => c.IsWeaverAssemblyReferenced(module))
                                             || t.HasCustomAttributes && t.CustomAttributes.Any(i => i.IsWeaverAssemblyReferenced(module)),
                    IModifierType t       => t.ElementType.IsWeaverAssemblyReferenced(module) || t.ModifierType.IsWeaverAssemblyReferenced(module),
                    FunctionPointerType t => ((IMethodSignature)t).IsWeaverAssemblyReferenced(module),
                    _                     => typeRef.Scope?.MetadataScopeType == MetadataScopeType.AssemblyNameReference && typeRef.Scope.Name == AssemblyName
                };
            }
        }
        
        public static bool IsWeaverAssemblyReferenced([NotNullWhen(true)] this TypeDefinition? typeDef, ModuleDefinition module)
        {
            if (typeDef == null)
                return false;

            return typeDef.IsWeaverAssemblyReferenced(module)
                   || typeDef.BaseType.IsWeaverAssemblyReferenced(module)
                   || typeDef.HasInterfaces && typeDef.Interfaces.Any(i => i.IsWeaverAssemblyReferenced(module))
                   || typeDef.HasGenericParameters && typeDef.GenericParameters.Any(i => i.IsWeaverAssemblyReferenced(module))
                   || typeDef.HasCustomAttributes && typeDef.CustomAttributes.Any(i => i.IsWeaverAssemblyReferenced(module))
                   || typeDef.HasMethods && typeDef.Methods.Any(i => i.IsWeaverAssemblyReferenced(module))
                   || typeDef.HasFields && typeDef.Fields.Any(i => i.IsWeaverAssemblyReferenced(module))
                   || typeDef.HasProperties && typeDef.Properties.Any(i => i.IsWeaverAssemblyReferenced(module))
                   || typeDef.HasEvents && typeDef.Events.Any(i => i.IsWeaverAssemblyReferenced(module));
        }

        public static bool IsWeaverAssemblyReferenced([NotNullWhen(true)] this IMethodSignature? method, ModuleDefinition module)
        {
            if (method == null)
                return false;

            if (method.ReturnType.IsWeaverAssemblyReferenced(module) || method.HasParameters && method.Parameters.Any(i => i.IsWeaverAssemblyReferenced(module)))
                return true;

            if (method is IGenericInstance genericInstance && genericInstance.HasGenericArguments && genericInstance.GenericArguments.Any(i => i.IsWeaverAssemblyReferenced(module)))
                return true;

            if (method is IGenericParameterProvider generic && generic.HasGenericParameters && generic.GenericParameters.Any(i => i.IsWeaverAssemblyReferenced(module)))
                return true;

            if (method is MethodReference methodRef)
            {
                if (methodRef is MethodDefinition methodDef)
                {
                    if (methodDef.HasCustomAttributes && methodDef.CustomAttributes.Any(i => i.IsWeaverAssemblyReferenced(module)))
                        return true;
                }
                else
                {
                    if (methodRef.DeclaringType.IsWeaverAssemblyReferenced(module))
                        return true;
                }
            }

            return false;
        }
        
        public static bool IsWeaverAssemblyReferenced([NotNullWhen(true)] this FieldReference? fieldRef, ModuleDefinition module)
        {
            if (fieldRef == null)
                return false;

            if (fieldRef.FieldType.IsWeaverAssemblyReferenced(module))
                return true;

            if (fieldRef is FieldDefinition fieldDef)
            {
                if (fieldDef.HasCustomAttributes && fieldDef.CustomAttributes.Any(i => i.IsWeaverAssemblyReferenced(module)))
                    return true;
            }
            else
            {
                if (fieldRef.DeclaringType.IsWeaverAssemblyReferenced(module))
                    return true;
            }

            return false;
        }
        
        public static bool IsWeaverAssemblyReferenced([NotNullWhen(true)] this PropertyReference? propRef, ModuleDefinition module)
        {
            if (propRef == null)
                return false;

            if (propRef.PropertyType.IsWeaverAssemblyReferenced(module))
                return true;

            if (propRef is PropertyDefinition propDef)
            {
                if (propDef.HasCustomAttributes && propDef.CustomAttributes.Any(i => i.IsWeaverAssemblyReferenced(module)))
                    return true;
            }
            else
            {
                if (propRef.DeclaringType.IsWeaverAssemblyReferenced(module))
                    return true;
            }

            return false;
        }
        
        public static bool IsWeaverAssemblyReferenced([NotNullWhen(true)] this EventReference? eventRef, ModuleDefinition module)
        {
            if (eventRef == null)
                return false;

            if (eventRef.EventType.IsWeaverAssemblyReferenced(module))
                return true;

            if (eventRef is EventDefinition eventDef)
            {
                if (eventDef.HasCustomAttributes && eventDef.CustomAttributes.Any(i => i.IsWeaverAssemblyReferenced(module)))
                    return true;
            }
            else
            {
                if (eventRef.DeclaringType.IsWeaverAssemblyReferenced(module))
                    return true;
            }

            return false;
        }
        
        public static bool IsWeaverAssemblyReferenced([NotNullWhen(true)] this ParameterDefinition? paramDef, ModuleDefinition module)
        {
            if (paramDef == null)
                return false;

            if (paramDef.ParameterType.IsWeaverAssemblyReferenced(module))
                return true;

            if (paramDef.HasCustomAttributes && paramDef.CustomAttributes.Any(i => i.IsWeaverAssemblyReferenced(module)))
                return true;

            return false;
        }
        
        public static bool IsWeaverAssemblyReferenced([NotNullWhen(true)] this CustomAttribute? attr, ModuleDefinition module)
        {
            if (attr == null)
                return false;

            if (attr.AttributeType.IsWeaverAssemblyReferenced(module))
                return true;

            if (attr.HasConstructorArguments && attr.ConstructorArguments.Any(i => i.Value is TypeReference typeRef && typeRef.IsWeaverAssemblyReferenced(module)))
                return true;

            if (attr.HasProperties && attr.Properties.Any(i => i.Argument.Value is TypeReference typeRef && typeRef.IsWeaverAssemblyReferenced(module)))
                return true;

            return false;
        }
        
        public static bool IsWeaverAssemblyReferenced([NotNullWhen(true)] this InterfaceImplementation? interfaceImpl, ModuleDefinition module)
        {
            if (interfaceImpl == null)
                return false;

            return interfaceImpl.InterfaceType.IsWeaverAssemblyReferenced(module)
                   || interfaceImpl.HasCustomAttributes && interfaceImpl.CustomAttributes.Any(i => i.IsWeaverAssemblyReferenced(module));
        }
        
        public static bool IsWeaverAssemblyReferenced([NotNullWhen(true)] this GenericParameterConstraint? constraint, ModuleDefinition module)
        {
            if (constraint == null)
                return false;

            if (constraint.ConstraintType.IsWeaverAssemblyReferenced(module))
                return true;

            if (constraint.HasCustomAttributes && constraint.CustomAttributes.Any(i => i.IsWeaverAssemblyReferenced(module)))
                return true;

            return false;
        }
    }
}
