using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using InterfaceBaseInvoke.Fody.Extensions;
using Mono.Cecil;

namespace InterfaceBaseInvoke.Fody.Support
{
    internal static class TypeHelper
    {
        public static MethodInfo GetMethod(Assembly assembly, string typeName, string methodName, bool isStatic, bool isPublic)
        {
            var type = assembly.GetType(typeName, true) ?? throw new TypeAccessException($"Cannot find type assembly {assembly.FullName} in by name " + typeName);
            var flag = isStatic ? BindingFlags.Static : BindingFlags.Instance;
            flag |= isPublic ? BindingFlags.Public : BindingFlags.NonPublic;
            var methods = type.GetMethods(flag).Where(m => m.Name == methodName).ToList();

            return methods.Count switch
            {
                0 => throw new MissingMethodException(type.FullName, methodName),
                > 1 => throw new AmbiguousMatchException($"Found more than one method in type {typeName} by name " + methodName),
                _ => methods[0]
            };
        }
    }
}
