using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractsRemover
{
	public class ContractCSharpFinder : SyntaxWalker
	{
		private readonly ContractInfo _contractInfo = new ContractInfo();

		private ContractCSharpFinder(MethodDeclarationSyntax node)
		{
			_contractInfo.Member = node;
		}

		/// <summary>
		/// Called when the walker visits a node.  This method may be overridden if subclasses want
		/// to handle the node.  Overrides should call back into this base method if they want the
		/// children of this node to be visited.
		/// </summary>
		/// <param name="node">The current node that the walker is visiting.</param>
		public override void Visit(SyntaxNode node)
		{
			if (!(node is InvocationExpressionSyntax invNode))
			{
				base.Visit(node);
				return;
			}

			var invInfo = new InvocationInfo(invNode);

			if (invInfo.Class == "Contract" && invInfo.Method == "Ensures")
			{
				_contractInfo.Ensures.Add(invNode);
			}
			else if (invInfo.Class == "Contract" && invInfo.Method == "Requires")
			{
				_contractInfo.Requires.Add(invNode);
			}
			else
			{
				base.Visit(node);
			}
		}

		public static ContractInfo Look(MethodDeclarationSyntax node)
		{
			if (node == null) throw new ArgumentNullException(nameof(node));

			var finder = new ContractCSharpFinder(node);
			finder.Visit(node);
			return finder._contractInfo;
		}
	}
}
