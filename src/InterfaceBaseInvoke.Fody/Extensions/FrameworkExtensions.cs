using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using InterfaceBaseInvoke.Fody.Models;
using Mono.Cecil;

namespace InterfaceBaseInvoke.Fody.Extensions
{
    internal static class FrameworkExtensions
    {
        public static TValue? GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            where TKey : notnull
            => dictionary.TryGetValue(key, out var value) ? value : default;

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
                collection.Add(item);
        }

        public static Expression Convert(this Expression e, Type type) => Expression.Convert(e, type);

        public static Expression<TDelegate> Lambda<TDelegate>(this Expression e, params ParameterExpression[] parameters) where TDelegate : Delegate
            => Expression.Lambda<TDelegate>(e, parameters);

        public static TypeReference ToTypeReference(this Type type, ModuleDefinition module)
        {
            return new TypeRefBuilder(module, type.Assembly.GetName().Name, type.FullName ?? type.Name).Build();
        }

        /// <summary>
        /// see details in https://stackoverflow.com/questions/7391348/c-sharp-clone-a-stack
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <returns></returns>
        public static Stack<T> Clone<T>(this Stack<T> original)
        {
            var arr = new T[original.Count];
            original.CopyTo(arr, 0);
            Array.Reverse(arr);
            return new Stack<T>(arr);
        }
    }
}
