using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InterfaceBaseInvoke.Tests.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace InterfaceBaseInvoke.Tests.SourceGenerator
{
    public class TestsSyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateTypes { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is not ClassDeclarationSyntax syntax)
                return;
            
            var name = syntax.Identifier.Text;

            if (name.EndsWith("TestCases"))
            {
                CandidateTypes.Add(syntax);
            }
        }
    }
}
