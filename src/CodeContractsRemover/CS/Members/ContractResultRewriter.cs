using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractsRemover.CS.Members
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
			var info = new InvocationInfo(node);
			if (info.Class == "Contract" && info.Method == "Result")
			{
				return SyntaxFactory.IdentifierName(_varName);
			}

			return base.VisitInvocationExpression(node);
		}
	}
}
