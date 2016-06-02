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
			var methodDecl = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
			if (methodDecl == null)
				return false;

			var methodParamNames = methodDecl.ParameterList.Parameters.Select(p => p.Identifier.ValueText).ToList();
			var assessExpression = node.Expression as MemberAccessExpressionSyntax;
			var contractTypeRef = assessExpression?.Expression as IdentifierNameSyntax;
			var contractMethodRef = assessExpression?.Name.Identifier.ValueText ?? "";
			var contractMethodTypeParams = (assessExpression?.Name as GenericNameSyntax)?.TypeArgumentList.Arguments.ToList();

			if (contractTypeRef?.Identifier.ValueText != "Contract" || ContractRemover.ContractMethods.Contains(contractMethodRef) == false)
				return false;

			if (this.mode == ContractReplacementMode.Convert && contractMethodRef == "Requires")
			{
				var nsPrefix = this.HasNamespace("System", node.SyntaxTree) ? "" : "System.";

				var checkExpression = node.ArgumentList.Arguments[0].Expression;
				var exceptionType = contractMethodTypeParams?.FirstOrDefault() ?? (isArgumentNullCheck(checkExpression, methodParamNames) ? SyntaxFactory.ParseTypeName(nsPrefix + "ArgumentNullException") : SyntaxFactory.ParseTypeName(nsPrefix + "ArgumentException"));
				var paramRef = (
					from n in checkExpression.DescendantNodes()
					let idSyntax = n as IdentifierNameSyntax
					let id = idSyntax?.Identifier.ValueText
					where id != null && methodParamNames.Contains(id)
					select id
				).FirstOrDefault() ?? "";

				replacementSyntax = SyntaxFactory.IfStatement(
					condition: SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, SyntaxFactory.ParenthesizedExpression(checkExpression)),
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

		private bool isArgumentNullCheck(ExpressionSyntax syntaxNode, List<string> paramNames)
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
	}
}
