﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using InterfaceBaseInvoke.Tests.SourceGenerator.Sources;
using Microsoft.CodeAnalysis;

namespace InterfaceBaseInvoke.Tests.SourceGenerator
{
    [Generator]
    public class TestsSourceGenerator : ISourceGenerator
    {
        internal const string RootNamespace = "InterfaceBaseInvoke.Tests.Weaving";
        internal const string GeneratedFilesHeader = "// <auto-generated />";
        internal static readonly string ReturnTypeName = typeof(ValueTuple<string, string>).Name;

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

            foreach (var type in types)
            {
                var idx = type.Name.IndexOf("TestCases", StringComparison.Ordinal);
                if (idx < 0)
                    continue;

                var usings = new List<string> { "Xunit", AssemblyName };

                using var builder = new SourceBuilder()
                                    .WriteLine(GeneratedFilesHeader)
                                    .WriteLine()
                                    .WriteUsings(usings)
                                    .WriteLine();

                // Namespace declaration
                builder.WriteLine($"namespace {RootNamespace}")
                       .WriteOpeningBracket();

                // Class declaration
                var className = type.Name.Substring(0, idx) + "Tests";
                builder.WriteLine($"public class {className} : ClassTestsBase")
                       .WriteOpeningBracket();

                builder.WriteLine($"protected override string ClassName => nameof({type.Name});");

                var methods = type.GetMembers().OfType<IMethodSymbol>();
                foreach (var method in methods.Where(m => m.DeclaredAccessibility == Accessibility.Public && m.MethodKind == MethodKind.Ordinary))
                {
                    if (method.ReturnType.Name != ReturnTypeName)
                        throw new InvalidOperationException();

                    builder.WriteLine("[Fact]")
                           .WriteLine($"public void {method.Name}()")
                           .WriteOpeningBracket();

                    builder.WriteLine($"var (expected, actual) = ((string, string))GetInstance().{method.Name}();")
                           .WriteLine("Assert.Equal(expected, actual);")
                           .WriteClosingBracket();
                }

                // End class declaration
                builder.WriteClosingBracket();

                // Standard Class declaration
                var classNameOfStandard = className + "Standard";
                builder.WriteLine($"public class {classNameOfStandard} : {className}")
                       .WriteOpeningBracket()
                       .WriteLine("protected override bool NetStandard => true;")
                // End class declaration
                .WriteClosingBracket();

                // End namespace declaration
                builder.WriteClosingBracket();

                var str = builder.ToString();
                context.AddSource($"{className}.g.cs", str);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            //#if DEBUG
            //            if (!Debugger.IsAttached)
            //            {
            //                Debugger.Launch();
            //            }
            //            Debug.WriteLine("Initalize code generator");
            //#endif
        }
    }
}
