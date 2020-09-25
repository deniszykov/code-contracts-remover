using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractsRemover
{
	public class ContractResultRewriter : CSharpSyntaxRewriter
	{
		private readonly string _varName;

		public ContractResultRewriter(string varName)
		{
			_varName = varName;
		}

		public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
		{
			var assessExpression = node.Expression as MemberAccessExpressionSyntax;
			var contractTypeRef = assessExpression?.Expression as IdentifierNameSyntax;
			var contractMethodRef = assessExpression?.Name.Identifier.ValueText ?? "";

			if (contractTypeRef?.Identifier.ValueText == "Contract" && contractMethodRef == "Result")
			{
				return SyntaxFactory.IdentifierName(_varName);
			}

			return base.VisitInvocationExpression(node);
		}
	}
}
