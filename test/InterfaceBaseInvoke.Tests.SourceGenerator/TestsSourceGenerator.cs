using System;
using System.Linq;
using System.Runtime.CompilerServices;
using InterfaceBaseInvoke.Tests.SourceGenerator.Sources;
using Microsoft.CodeAnalysis;

namespace InterfaceBaseInvoke.Tests.SourceGenerator
{
    [Generator]
    public class TestsSourceGenerator : IIncrementalGenerator
    {
        public static readonly string[] Namespaces =
        [
            "InterfaceBaseInvoke",
            "Tests",
            "AssemblyToProcess",
        ];

        public static readonly string AssemblyName = string.Join(".", Namespaces);

        public static void Execute(SourceProductionContext context, Compilation compilation)
        {
            try
            {
                ExecuteInternal(context, compilation);
            }
            catch (Exception ex)
            {
                const string name = "InterfaceBaseInvoke.Tests";
                var descriptor = new DiagnosticDescriptor(
                    id: "IBIT001",
                    title: $"An exception was thrown by the {name} generator",
                    messageFormat: "An exception was thrown by the {0} generator: '{1}'",
                    category: name,
                    defaultSeverity: DiagnosticSeverity.Error,
                    isEnabledByDefault: true);

                context.ReportDiagnostic(Diagnostic.Create(descriptor: descriptor,
                                                           location: Location.None,
                                                           messageArgs: [name, ex.ToString()]));
            }
        }

        //By not inlining we make sure we can catch assembly loading errors when jitting this method
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ExecuteInternal(SourceProductionContext context, Compilation compilation)
        {
            var assemblySymbol = compilation.SourceModule.ReferencedAssemblySymbols.FirstOrDefault(q => q.Name == AssemblyName);
            if (assemblySymbol == null)
                throw new InvalidOperationException("Cannot find assembly symbol: " + AssemblyName);

            var cur = Namespaces.Aggregate(assemblySymbol.GlobalNamespace, (current, ns) => current.GetNamespaceMembers().First(m => m.Name == ns));
            var types = cur.GetTypeMembers();

            foreach (var type in types)
            {
                var idx = type.Name.IndexOf("TestCases", StringComparison.Ordinal);
                if (idx < 0)
                    continue;

                var className = type.Name.Substring(0, idx) + "Tests";
                var (name, code) = TestsSource.Generate(type, className);
                context.AddSource(name, code);
            }
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //if (Debugger.IsAttached == false)
            //    Debugger.Launch();

            context.RegisterImplementationSourceOutput(context.CompilationProvider, Execute);
        }
    }
}
