using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CodeContractsRemover
{
	public class ContractCSharpSyntaxRewriter : CSharpSyntaxRewriter
	{
		private readonly ContractReplacementMode _mode;

		public ContractCSharpSyntaxRewriter(ContractReplacementMode mode)
		{
			this._mode = mode;
		}

		public override SyntaxNode VisitUsingDirective(UsingDirectiveSyntax node)
		{
			if (node.Name.GetText().ToString() == "System.Diagnostics.Contracts")
			{
				return null;
			}

			return base.VisitUsingDirective(node);
		}

		public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
		{
			// remove [ContractClassForAttribute]
			if (HasAttribute(node.AttributeLists, ContractRemover.ContractClassForAttributeName))
				return null;

			return base.VisitClassDeclaration(node);
		}

		public override SyntaxNode VisitAttribute(AttributeSyntax node)
		{
			if (ContractRemover.ContractAttributes.Contains(node.Name.ToString()))
				return null;

			return base.VisitAttribute(node);
		}

		public override SyntaxNode VisitAttributeList(AttributeListSyntax node)
		{
			node = (AttributeListSyntax)base.VisitAttributeList(node);

			if (node.Attributes.Count == 0)
				return null;
			return node;
		}

		private bool HasAttribute(SyntaxList<AttributeListSyntax> attributes, string attributeName)
		{
			return attributes.Any(al => al.Attributes.Select(a => a.Name.ToString()).Any(n => n == attributeName || n == attributeName + "Attribute"));
		}

		/// <summary>Called when the visitor visits a MethodDeclarationSyntax node.</summary>
		public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
		{
			var contractInfo = ContractCSharpFinder.Look(node);
			if (contractInfo.HasContract)
			{
				var visitor = new CSharpMethodRewriter(contractInfo, _mode);
				return visitor.Visit(node);
			}

			return base.VisitMethodDeclaration(node);
		}
	}
}
