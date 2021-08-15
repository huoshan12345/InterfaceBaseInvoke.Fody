using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace InterfaceBaseInvoke.Fody.Support
{
    public static class DelegateHelper
    {
        public static T CreateDelegate<T>(MethodInfo method) where T : Delegate
        {
            return (T)Delegate.CreateDelegate(typeof(T), method);
        }

        public static T CreateDelegate<T>(Type type, string methodName) where T : Delegate
        {
            var method = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                             .FirstOrDefault(m => m.Name == methodName);
            return CreateDelegate<T>(method ?? throw new MissingMethodException(type.FullName, methodName));
        }

        public static T CreateDelegate<T>(Assembly assembly, string typeName, string methodName) where T : Delegate
        {
            var type = assembly.GetType(typeName, true)!;
            return CreateDelegate<T>(type, methodName);
        }
    }
}
