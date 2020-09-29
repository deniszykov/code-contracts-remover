using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CodeContractsRemover.CS
{
	public static class SyntaxExt
	{
		public static bool IsStatic(this MethodDeclarationSyntax node)
		{
			return node.Modifiers.Any(m => m.Value.ToString() == "static");
		}

		public static bool IsStatic(this ConstructorDeclarationSyntax node)
		{
			return node.Modifiers.Any(m => m.Value.ToString() == "static");
		}

		public static bool IsStatic(this PropertyDeclarationSyntax node)
		{
			return node.Modifiers.Any(m => m.Value.ToString() == "static");
		}

		public static bool IsOverride(this MethodDeclarationSyntax node)
		{
			return node.Modifiers.Any(m => m.Value.ToString() == "override");
		}

		public static bool IsOverride(this PropertyDeclarationSyntax node)
		{
			return node.Modifiers.Any(m => m.Value.ToString() == "override");
		}

		public static bool IsNullCheck(this ExpressionSyntax syntaxNode)
		{
			var binaryExpression = syntaxNode as BinaryExpressionSyntax;
			if (binaryExpression == null)
				return false;

			var literalExpression = (binaryExpression.Left as LiteralExpressionSyntax) ?? (binaryExpression.Right as LiteralExpressionSyntax);

			if (literalExpression == null)
				return false;

			return literalExpression.Token.Text == "null";
		}

		public static bool IsRangeCheck(this ExpressionSyntax syntaxNode)
		{
			var binaryExpression = syntaxNode as BinaryExpressionSyntax;
			if (binaryExpression == null)
				return false;

			var operatorKind = binaryExpression.OperatorToken.Kind();
			var paramExpression = (binaryExpression.Left as IdentifierNameSyntax) ?? (binaryExpression.Right as IdentifierNameSyntax);
			var literalExpression = (binaryExpression.Left as LiteralExpressionSyntax) ?? (binaryExpression.Right as LiteralExpressionSyntax);

			if (paramExpression == null || literalExpression == null)
				return false;

			return (operatorKind == SyntaxKind.GreaterThanEqualsToken ||
			        operatorKind == SyntaxKind.GreaterThanToken ||
			        operatorKind == SyntaxKind.LessThanEqualsToken ||
			        operatorKind == SyntaxKind.LessThanToken);
		}

		public static SyntaxList<AttributeListSyntax> AddAttr(this SyntaxList<AttributeListSyntax> attributeLists, string attrName)
		{
			var notNullAttr = AttributeList(SeparatedList(new[] { Attribute(ParseName(attrName)) }));
			return List(attributeLists.Add(notNullAttr));
		}
	}
}
