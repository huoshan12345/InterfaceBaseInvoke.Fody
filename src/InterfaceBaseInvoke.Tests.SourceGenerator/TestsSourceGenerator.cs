using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis;

namespace InterfaceBaseInvoke.Tests.SourceGenerator
{
    [Generator]
    public class TestsSourceGenerator : ISourceGenerator
    {
        public static readonly string[] Namespaces =
        {
            "InterfaceBaseInvoke",
            "Tests",
            "AssemblyToProcess"
        };

        public static readonly string AssemblyName = string.Join(".", Namespaces);

        public void Execute(GeneratorExecutionContext context)
        {
            var assemblySymbol = context.Compilation.SourceModule.ReferencedAssemblySymbols.FirstOrDefault(q => q.Name == AssemblyName);
            if (assemblySymbol == null)
                throw new InvalidOperationException("Cannot find assembly symbol: " + AssemblyName);

            var cur = Namespaces.Aggregate(assemblySymbol.GlobalNamespace, (current, ns) => current.GetNamespaceMembers().First(m => m.Name == ns));
            var types = cur.GetTypeMembers();

            foreach (var type in types.Where(m => m.Name.EndsWith("TestCases")))
            {

            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
            Debug.WriteLine("Initalize code generator");
#endif 
        }
    }
}
