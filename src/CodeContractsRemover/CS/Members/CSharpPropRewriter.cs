using System;
using System.Linq;
using CodeContractsRemover.CS.Class;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractsRemover.CS.Members
{
	public class CSharpPropRewriter : CSharpSyntaxRewriter
	{
		private readonly MemberContractInfo _propContract;
		private readonly ClassContractInfo _classContract;
		private readonly ContractReplacementMode _mode;

		private CSharpPropRewriter(MemberContractInfo propContract, ClassContractInfo classContract,
			ContractReplacementMode mode)
		{
			var modes = new[] { ContractReplacementMode.Convert, ContractReplacementMode.ConvertAndAddAnnotations };
			if (!modes.Contains(mode))
				throw new ArgumentOutOfRangeException(nameof(mode), mode, $"Use only {string.Join(", ", modes)}");

			propContract.Init();
			_propContract = propContract;
			_classContract = classContract;
			_mode = mode;
		}

		public static SyntaxNode Visit(PropertyDeclarationSyntax node, ClassContractInfo classContract,
			ContractReplacementMode mode)
		{
			var propContract = new MemberContractInfo(node);
			return new CSharpPropRewriter(propContract, classContract, mode).Visit(node);
		}

		/// <summary>Called when the visitor visits a PropertyDeclarationSyntax node.</summary>
		public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
		{
			var proxy = new PropertyDeclarationSyntaxProxy(node);
			if (_mode == ContractReplacementMode.ConvertAndAddAnnotations)
			{
				new CommonRewriterLogic(_propContract).AddAnnotationAttrs(
					proxy, new TriviaInfo(node), node.GetLeadingTrivia());
			}

			return base.VisitPropertyDeclaration(proxy.Node);
		}

		/// <summary>Called when the visitor visits a AccessorDeclarationSyntax node.</summary>
		public override SyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
		{
			return CSharpMemberRewriter.Visit(node, _classContract, _mode);
		}
	}
}
