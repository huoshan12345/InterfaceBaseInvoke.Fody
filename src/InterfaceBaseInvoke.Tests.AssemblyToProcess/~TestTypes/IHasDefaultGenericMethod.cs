using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public interface IHasDefaultGenericMethod
    {
        string Method(int x, string y) => $"{nameof(IHasDefaultMethod)}.{nameof(Method)}.{x}.{y}";
        string Method<T>(int x, string y) => $"{nameof(IHasDefaultMethod)}.{nameof(Method)}.{x}.{y}.{typeof(T).Name}";
    }
}
