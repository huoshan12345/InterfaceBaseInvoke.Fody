﻿using System;

namespace InterfaceBaseInvoke;

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
    /// <returns></returns>
    public static TInterface Base<TInterface>(this TInterface instance) =>
        throw new InvalidOperationException("This method is meant to be replaced at compile time by InterfaceBaseInvoke.Fody, but the weaver has not been executed correctly.");
}
