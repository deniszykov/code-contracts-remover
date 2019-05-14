using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractsRemover
{
	public class ContractCSharpMethodCallFinder : CSharpSyntaxVisitor
	{
		private bool found;

	    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
	    {
			var assessExpression = node.Expression as MemberAccessExpressionSyntax;
			var contractTypeRef = assessExpression?.Expression as IdentifierNameSyntax;
			var contractMethodRef = assessExpression?.Name.Identifier.ValueText ?? "";

			if (contractTypeRef?.Identifier.ValueText == "Contract" && ContractRemover.ContractMethods.Contains(contractMethodRef))
				this.found = true;

			base.VisitInvocationExpression(node);
	    }

		public static bool Look(SyntaxNode node)
		{
		    if (node == null) throw new ArgumentNullException(nameof(node));

		    var finder = new ContractCSharpMethodCallFinder();
			finder.Visit(node);
			return finder.found;
		}
	}
}
