using System;
using System.Linq;
using CodeContractsRemover.CS.Members;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractsRemover.CS.Class
{
	public class CSharpClassRewriter : CSharpSyntaxRewriter
	{
		private readonly ContractReplacementMode _mode;
		private readonly ClassContractInfo _classContract;

		private CSharpClassRewriter(ClassDeclarationSyntax node, ContractReplacementMode mode)
		{
			var modes = new[] { ContractReplacementMode.Convert, ContractReplacementMode.ConvertAndAddAnnotations };
			if (!modes.Contains(mode))
				throw new ArgumentOutOfRangeException(nameof(mode), mode, $"Use only {string.Join(", ", modes)}");

			_mode = mode;
			_classContract = CSharpClassContractFinder.GetContract(node);
		}

		public static SyntaxNode Visit(ClassDeclarationSyntax node, ContractReplacementMode mode)
		{
			return ((CSharpSyntaxRewriter)new CSharpClassRewriter(node, mode)).Visit(node);
		}

		/// <summary>Called when the visitor visits a MethodDeclarationSyntax node.</summary>
		public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
		{
			return CSharpMemberRewriter.Visit(node, _classContract, _mode);
		}

		/// <summary>Called when the visitor visits a ConstructorDeclarationSyntax node.</summary>
		public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
		{
			return CSharpMemberRewriter.Visit(node, _classContract, _mode);
		}

		/// <summary>Called when the visitor visits a PropertyDeclarationSyntax node.</summary>
		public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
		{
			return CSharpPropRewriter.Visit(node, _classContract, _mode);
		}
	}
}
