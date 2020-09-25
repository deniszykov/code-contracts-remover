using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractsRemover
{
	public class CheckInfo
	{
		public CheckInfo(InvocationExpressionSyntax node)
		{
			CheckExpression = node.ArgumentList.Arguments[0].Expression;
			MessageExpression = node.ArgumentList.Arguments.Count > 1
				? node.ArgumentList.Arguments[1].Expression
				: SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
					SyntaxFactory.Literal("Contract assertion not met: " + CheckExpression.ToString()));
		}

		public ExpressionSyntax CheckExpression { get; private set; }
		public ExpressionSyntax MessageExpression { get; private set; }
	}
}
