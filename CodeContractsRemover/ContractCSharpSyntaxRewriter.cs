using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractsRemover
{
	public class ContractCSharpSyntaxRewriter : CSharpSyntaxRewriter
	{
		private readonly ContractReplacementMode mode;

		public ContractCSharpSyntaxRewriter(ContractReplacementMode mode)
		{
			this.mode = mode;
		}

		public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
		{
			var replacementSyntax = default(StatementSyntax);
			if (node.Expression is InvocationExpressionSyntax && VisitInvocationExpression((InvocationExpressionSyntax)node.Expression, out replacementSyntax))
				return replacementSyntax;

			return base.VisitExpressionStatement(node);
		}

		private bool VisitInvocationExpression(InvocationExpressionSyntax node, out StatementSyntax replacementSyntax)
		{
			replacementSyntax = null;

			var assessExpression = node.Expression as MemberAccessExpressionSyntax;
			var contractTypeRef = assessExpression?.Expression as IdentifierNameSyntax;
			var contractMethodRef = assessExpression?.Name.Identifier.ValueText ?? "";
			var contractMethodTypeParams = (assessExpression?.Name as GenericNameSyntax)?.TypeArgumentList.Arguments.ToList();

			if (contractTypeRef?.Identifier.ValueText != "Contract" || ContractRemover.ContractMethods.Contains(contractMethodRef) == false)
				return false;

			var methodParamNames = new List<string>();
			var methodDecl = node.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>();
			var propDecl = node.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
			var eventDecl = node.FirstAncestorOrSelf<EventDeclarationSyntax>();
			if (methodDecl != null)
				methodParamNames.AddRange(methodDecl.ParameterList.Parameters.Select(p => p.Identifier.ValueText).ToList());
			else if (propDecl != null || eventDecl != null)
				methodParamNames.Add("value");
			else
				return false;

			if (this.mode == ContractReplacementMode.Convert && contractMethodRef == "Requires")
			{
				var nsPrefix = this.HasNamespace("System", node.SyntaxTree) ? "" : "System.";

				var checkExpression = node.ArgumentList.Arguments[0].Expression;
				var exceptionType = contractMethodTypeParams?.FirstOrDefault() ??
				(
						IsArgumentNullCheck(checkExpression, methodParamNames) ? SyntaxFactory.ParseTypeName(nsPrefix + "ArgumentNullException") :
						IsRangeCheck(checkExpression, methodParamNames) ? SyntaxFactory.ParseTypeName(nsPrefix + "ArgumentOutOfRangeException") :
						SyntaxFactory.ParseTypeName(nsPrefix + "ArgumentException")
				);
				var paramRef = (
					from n in checkExpression.DescendantNodes()
					let idSyntax = n as IdentifierNameSyntax
					let id = idSyntax?.Identifier.ValueText
					where id != null && methodParamNames.Contains(id)
					select id
				).FirstOrDefault() ?? "";

				replacementSyntax = SyntaxFactory.IfStatement(
					condition: InverseExpression(checkExpression),
					statement: SyntaxFactory.Block(
						SyntaxFactory.ThrowStatement(
							SyntaxFactory.ObjectCreationExpression(
								type: exceptionType,
								argumentList: SyntaxFactory.ArgumentList(
									new SeparatedSyntaxList<ArgumentSyntax>()
										.Add(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(paramRef))))
										.Add(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(checkExpression.ToString()))))

								),
								initializer: null
							)
						)
					)
				).NormalizeWhitespace(eol: string.Empty, indentation: " ").WithTriviaFrom(node.Parent);
			}
			else if (this.mode == ContractReplacementMode.Convert && (contractMethodRef == "Assert" || contractMethodRef == "Assume"))
			{
				var nsPrefix = this.HasNamespace("System.Diagnostics", node.SyntaxTree) ? "" : "System.Diagnostics.";

				var checkExpression = node.ArgumentList.Arguments[0].Expression;
				var messageExpression = node.ArgumentList.Arguments.ElementAtOrDefault(1)?.Expression;

				replacementSyntax = SyntaxFactory.ExpressionStatement(
					SyntaxFactory.InvocationExpression(
						expression: SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ParseTypeName(nsPrefix + "Debug"), SyntaxFactory.IdentifierName("Assert")),
						argumentList: SyntaxFactory.ArgumentList(
							new SeparatedSyntaxList<ArgumentSyntax>()
								.Add(SyntaxFactory.Argument(checkExpression))
								.Add(SyntaxFactory.Argument(messageExpression ?? SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(checkExpression.ToString()))))
						)
					)
				).NormalizeWhitespace(eol: string.Empty, indentation: " ").WithTriviaFrom(node.Parent);
			}

			return true;
		}

		private bool HasNamespace(string namespaceName, SyntaxTree tree)
		{
			return (from n in tree.GetRoot().ChildNodes()
					let nsblock = n as NamespaceDeclarationSyntax
					where nsblock != null
					from us in nsblock.Usings
					where us.Name.GetText().ToString() == namespaceName
					select us).Any();
		}

		private bool IsArgumentNullCheck(ExpressionSyntax syntaxNode, List<string> paramNames)
		{
			var binaryExpression = syntaxNode as BinaryExpressionSyntax;
			if (binaryExpression == null)
				return false;

			var paramExpression = (binaryExpression.Left as IdentifierNameSyntax) ?? (binaryExpression.Right as IdentifierNameSyntax);
			var literalExpression = (binaryExpression.Left as LiteralExpressionSyntax) ?? (binaryExpression.Right as LiteralExpressionSyntax);

			if (paramExpression == null || literalExpression == null)
				return false;

			return paramNames.Contains(paramExpression.Identifier.ValueText) && literalExpression.Token.Text == "null";
		}
		private bool IsRangeCheck(ExpressionSyntax syntaxNode, List<string> paramNames)
		{
			var binaryExpression = syntaxNode as BinaryExpressionSyntax;
			if (binaryExpression == null)
				return false;

			var operatorKind = binaryExpression.OperatorToken.Kind();
			var paramExpression = (binaryExpression.Left as IdentifierNameSyntax) ?? (binaryExpression.Right as IdentifierNameSyntax);
			var literalExpression = (binaryExpression.Left as LiteralExpressionSyntax) ?? (binaryExpression.Right as LiteralExpressionSyntax);

			if (paramExpression == null || literalExpression == null)
				return false;

			return paramNames.Contains(paramExpression.Identifier.ValueText) &&
				   (operatorKind == SyntaxKind.GreaterThanEqualsToken ||
					operatorKind == SyntaxKind.GreaterThanToken ||
					operatorKind == SyntaxKind.LessThanEqualsToken ||
					operatorKind == SyntaxKind.LessThanToken);
		}
		private ExpressionSyntax InverseExpression(ExpressionSyntax checkExpression)
		{
			if (checkExpression == null) throw new ArgumentNullException(nameof(checkExpression));

			var binaryExpression = checkExpression as BinaryExpressionSyntax;
			if (binaryExpression == null)
				return InverseExpressionWithNot(checkExpression);

			var operatorKind = binaryExpression.OperatorToken.Kind();
			var isSimpleExpression = (binaryExpression.Left is IdentifierNameSyntax || binaryExpression.Right is IdentifierNameSyntax) &&
									 (binaryExpression.Left is LiteralExpressionSyntax || binaryExpression.Right is LiteralExpressionSyntax);

			if (!isSimpleExpression)
				return InverseExpressionWithNot(checkExpression);

			switch (operatorKind)
			{
				case SyntaxKind.EqualsEqualsToken: return binaryExpression.WithOperatorToken(SyntaxFactory.Token(SyntaxKind.ExclamationEqualsToken));
				case SyntaxKind.ExclamationEqualsToken: return binaryExpression.WithOperatorToken(SyntaxFactory.Token(SyntaxKind.EqualsEqualsToken));
				case SyntaxKind.GreaterThanToken: return binaryExpression.WithOperatorToken(SyntaxFactory.Token(SyntaxKind.LessThanEqualsToken));
				case SyntaxKind.LessThanToken: return binaryExpression.WithOperatorToken(SyntaxFactory.Token(SyntaxKind.GreaterThanEqualsToken));
				case SyntaxKind.GreaterThanEqualsToken: return binaryExpression.WithOperatorToken(SyntaxFactory.Token(SyntaxKind.LessThanToken));
				case SyntaxKind.LessThanEqualsToken: return binaryExpression.WithOperatorToken(SyntaxFactory.Token(SyntaxKind.GreaterThanToken));

				default: return InverseExpressionWithNot(checkExpression);
			}
		}

		private ExpressionSyntax InverseExpressionWithNot(ExpressionSyntax checkExpression)
		{
			if (checkExpression == null) throw new ArgumentNullException(nameof(checkExpression));

			return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, SyntaxFactory.ParenthesizedExpression(checkExpression));
		}
	}
}
