using System;
using System.Linq;
using CodeContractsRemover.CS.Class;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CodeContractsRemover.CS
{
	public class ContractCSharpSyntaxRewriter : CSharpSyntaxRewriter
	{
		private readonly ContractReplacementMode _mode;

		public ContractCSharpSyntaxRewriter(ContractReplacementMode mode)
		{
			var modes = new[] {ContractReplacementMode.Convert, ContractReplacementMode.ConvertAndAddAnnotations};
			if(!modes.Contains(mode))
				throw new ArgumentOutOfRangeException(nameof(mode), mode, $"Mode '${mode}' is not supported. Use only {string.Join(", ", modes)}");

			_mode = mode;
		}

		public override SyntaxNode VisitUsingDirective(UsingDirectiveSyntax node)
		{
			if (node.Name.GetText().ToString() == "System.Diagnostics.Contracts")
			{
				if (_mode == ContractReplacementMode.ConvertAndAddAnnotations)
					return UsingDirective(
							QualifiedName(IdentifierName("JetBrains"), IdentifierName("Annotations")))
						.NormalizeWhitespace()
						.WithLeadingTrivia(node.GetLeadingTrivia())
						.WithTrailingTrivia(node.GetTrailingTrivia());

				return null;
			}

			return base.VisitUsingDirective(node);
		}

		public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
		{
			return CSharpClassRewriter.Visit(node, _mode);
		}
	}
}
