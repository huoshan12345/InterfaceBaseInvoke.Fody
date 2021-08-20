using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfaceBaseInvoke.Tests.AssemblyToProcess
{
    public interface IGenericInterface<T>
    {
        string Method(int x, string y) => $"{nameof(IHasDefaultMethod)}.{nameof(Method)}.{x}.{y}.{typeof(T).Name}";
        string Method<TParameter>(int x, string y) => $"{nameof(IHasDefaultMethod)}.{nameof(Method)}.{x}.{y}.{typeof(TParameter).Name}";
    }
}
