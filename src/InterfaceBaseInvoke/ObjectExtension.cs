using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace InterfaceBaseInvoke
{
    /// <summary>
    /// 
    /// </summary>
    public static class ObjectExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="instance"></param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        public static void Base<TInterface>(this TInterface instance, string methodName, params object?[] args) => Throw();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="instance"></param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static TReturn Base<TInterface, TReturn>(this TInterface instance, string methodName, params object?[] args) => throw Throw();

        internal static Exception Throw() =>
            throw new InvalidOperationException("This method is meant to be replaced at compile time by InterfaceBaseInvoke.Fody, but the weaver has not been executed correctly.");
    }
}
