using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CodeContractsRemover
{
	public class CSharpMethodRewriter : CSharpSyntaxRewriter
	{
		private readonly ContractInfo _contractInfo;
		private readonly ContractReplacementMode _mode;

		public CSharpMethodRewriter(ContractInfo contractInfo, ContractReplacementMode mode)
		{
			_contractInfo = contractInfo;
			_mode = mode;
		}

		/// <summary>Called when the visitor visits a MethodDeclarationSyntax node.</summary>
		public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
		{
			if (_contractInfo.Requires.Any())
			{
				var reqStatements = new List<StatementSyntax>(_contractInfo.Requires.Count);
				foreach (var require in _contractInfo.Requires)
				{
					reqStatements.Add(GenerateArgException(require, new InvocationInfo(require)));
				}

				var statements = node.Body.Statements.InsertRange(0, reqStatements);
				node = node.WithBody(node.Body.WithStatements(new SyntaxList<StatementSyntax>(statements)));
			}

			var returnType = (node.ReturnType as PredefinedTypeSyntax)?.Keyword;
			if (_contractInfo.Ensures.Any() && returnType != null && returnType.Value.IsKind(SyntaxKind.VoidKeyword))
			{
				var ensStatements = GenerateEnsures(null, new TriviaInfo(node.Body).AddIndent());

				var statements = node.Body.Statements.AddRange(ensStatements);
				node = node.WithBody(node.Body.WithStatements(new SyntaxList<StatementSyntax>(statements)));
			}

			return base.VisitMethodDeclaration(node);
		}

		public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
		{
			var invNode = node.Expression as InvocationExpressionSyntax;
			if (invNode == null)
			{
				return base.VisitExpressionStatement(node);
			}

			var invInfo = new InvocationInfo(invNode);
			if (_mode == ContractReplacementMode.Convert
			    && invInfo.Class == "Contract"
			    && (invInfo.Method == "Assert" || invInfo.Method == "Assume"))
			{
				return GenerateArgException(invNode, invInfo);
			}
			else if (_mode == ContractReplacementMode.Convert
			         && invInfo.Class == "Contract" &&
			         (invInfo.Method == "Requires" || invInfo.Method == "Ensures"))
			{
				// All requires have been added to the beginning of method in VisitMethodDeclaration
				// Ensures are rewrited in VisitMethodDeclaration and VisitReturn
				return null;
			}

			return base.VisitExpressionStatement(node);
		}

		private StatementSyntax GenerateArgException(InvocationExpressionSyntax node, InvocationInfo invInfo)
		{
			var nsPrefix = GetNsPrefix(_contractInfo.Member);
			var checkInfo = new CheckInfo(node);
			var identifiers = GetIdentifiers(_contractInfo.Member);
			var exceptionType = invInfo.GenericArgs?.FirstOrDefault() ??
			                    (
				                    IsArgumentNullCheck(checkInfo.CheckExpression, identifiers)
					                    ?
					                    ParseTypeName(nsPrefix + "ArgumentNullException")
					                    :
					                    IsRangeCheck(checkInfo.CheckExpression, identifiers)
						                    ? ParseTypeName(nsPrefix + "ArgumentOutOfRangeException")
						                    :
						                    ParseTypeName(nsPrefix + "ArgumentException")
			                    );
			var paramRef = checkInfo.CheckExpression.DescendantNodes().OfType<IdentifierNameSyntax>()
				.Where(ids => identifiers.Contains(ids.Identifier.ValueText))
				.FirstOrDefault();
			var paramNameRef = paramRef != null
				? (ExpressionSyntax) InvocationExpression(IdentifierName("nameof"))
					.WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(paramRef))))
				: LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("value"));

			var exceptionParams = new[] {paramNameRef, checkInfo.MessageExpression};

			if (exceptionType.ToString().EndsWith(".ArgumentException", StringComparison.Ordinal) ||
			    exceptionType.ToString().Equals("ArgumentException", StringComparison.Ordinal))
			{
				Array.Reverse(exceptionParams); // ArgumentException has reverse params order
			}

			return GenerateThrow(checkInfo.CheckExpression, exceptionType, new TriviaInfo(node.Parent),
				exceptionParams);
		}

		private static List<string> GetIdentifiers(SyntaxNode node)
		{
			var identifiers = new List<string>();
			var methodDecl = node.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>();
			var propDecl = node.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
			var indexDecl = node.FirstAncestorOrSelf<IndexerDeclarationSyntax>();
			var eventDecl = node.FirstAncestorOrSelf<EventDeclarationSyntax>();
			if (methodDecl != null)
				identifiers.AddRange(methodDecl.ParameterList.Parameters.Select(p => p.Identifier.ValueText).ToList());
			else if (propDecl != null || eventDecl != null)
				identifiers.Add("value");
			else if (indexDecl != null)
			{
				identifiers.AddRange(indexDecl.ParameterList.Parameters.Select(p => p.Identifier.ValueText).ToList());
				identifiers.Add("value");
			}

			return identifiers;
		}

		private string GetNsPrefix(SyntaxNode node)
		{
			var nsPrefix = this.HasNamespace("System", node.SyntaxTree) ? "" : "System.";
			return nsPrefix;
		}

		private StatementSyntax GenerateThrow(ExpressionSyntax checkExpression,
			TypeSyntax exceptionType, TriviaInfo trivias, params ExpressionSyntax[] exceptionParams)
		{
			var exArgs = new SeparatedSyntaxList<ArgumentSyntax>();
			foreach (var par in exceptionParams)
			{
				exArgs = exArgs.Add(Argument(par));
			}

			return IfStatement(
					condition: InverseExpression(checkExpression),
					statement: Block(
							ThrowStatement(
									ObjectCreationExpression(
										type: exceptionType,
										argumentList: ArgumentList(exArgs),
										initializer: null
									).NormalizeWhitespace()
								)
								.WithThrowKeyword(Token(trivias.IndentedWhitespace, SyntaxKind.ThrowKeyword, trivias.SpaceList))
								.WithSemicolonToken(Token(TriviaList(), SyntaxKind.SemicolonToken, trivias.EndOfLine))
						)
						.WithOpenBraceToken(Token(trivias.BaseWhitespace, SyntaxKind.OpenBraceToken, trivias.EndOfLine))
						.WithCloseBraceToken(Token(trivias.BaseWhitespace, SyntaxKind.CloseBraceToken, trivias.EndOfLine))
				)
				.WithIfKeyword(Token(trivias.LeadingTrivia, SyntaxKind.IfKeyword, trivias.SpaceList))
				.WithCloseParenToken(Token(TriviaList(), SyntaxKind.CloseParenToken, trivias.EndOfLine));
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

			var logicalNotExpression = checkExpression as PrefixUnaryExpressionSyntax;
			if (logicalNotExpression != null
				&& logicalNotExpression.OperatorToken.Kind() == SyntaxKind.ExclamationToken)
			{
				return logicalNotExpression.Operand;
			}

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
				case SyntaxKind.EqualsEqualsToken: return binaryExpression.WithOperatorToken(Token(SyntaxKind.ExclamationEqualsToken).WithTriviaFrom(binaryExpression.OperatorToken));
				case SyntaxKind.ExclamationEqualsToken: return binaryExpression.WithOperatorToken(Token(SyntaxKind.EqualsEqualsToken).WithTriviaFrom(binaryExpression.OperatorToken));
				//case SyntaxKind.GreaterThanToken: return binaryExpression.WithOperatorToken(Token(SyntaxKind.LessThanEqualsToken));
				//case SyntaxKind.LessThanToken: return binaryExpression.WithOperatorToken(Token(SyntaxKind.GreaterThanEqualsToken));
				//case SyntaxKind.GreaterThanEqualsToken: return binaryExpression.WithOperatorToken(Token(SyntaxKind.LessThanToken));
				//case SyntaxKind.LessThanEqualsToken: return binaryExpression.WithOperatorToken(Token(SyntaxKind.GreaterThanToken));

				default: return InverseExpressionWithNot(checkExpression);
			}
		}

		private ExpressionSyntax InverseExpressionWithNot(ExpressionSyntax checkExpression)
		{
			if (checkExpression == null) throw new ArgumentNullException(nameof(checkExpression));

			return PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, ParenthesizedExpression(checkExpression));
		}


		private bool HasNamespace(string namespaceName, SyntaxTree tree)
		{
			if (tree.GetRoot().ChildNodes().OfType<UsingDirectiveSyntax>().Any(uds => uds.Name.GetText().ToString() == namespaceName))
			{
				return true;
			}

			return (from n in tree.GetRoot().ChildNodes()
				let nsblock = n as NamespaceDeclarationSyntax
				where nsblock != null
				from us in nsblock.Usings
				where us.Name.GetText().ToString() == namespaceName
				select us).Any();
		}

		/// <summary>Called when the visitor visits a ReturnStatementSyntax node.</summary>
		public override SyntaxNode VisitReturnStatement(ReturnStatementSyntax node)
		{
			var varName = $"resultOf{_contractInfo.Member.Identifier.Text}";
			var trivias = new TriviaInfo(node);

			var statements = new List<StatementSyntax>(_contractInfo.Ensures.Count + 2);

			var isVoidMethod = node.Expression == null;
			if (!isVoidMethod)
			{
				statements.Add(VarDeclaraion(varName, node.Expression, trivias));
			}

			var triviaWithIndent = trivias.AddIndent();
			statements.AddRange(GenerateEnsures(varName, triviaWithIndent));

			if (isVoidMethod)
			{
				statements.Add(ReturnResult(trivias));
			}
			else
			{
				statements.Add(ReturnResult(varName, trivias));
			}

			return Block(statements)
				.WithOpenBraceToken(Token(trivias.LeadingTrivia, SyntaxKind.OpenBraceToken, trivias.EndOfLine))
				.WithCloseBraceToken(Token(trivias.BaseWhitespace, SyntaxKind.CloseBraceToken, trivias.EndOfLine));
		}

		private List<StatementSyntax> GenerateEnsures(string varName, TriviaInfo trivias)
		{
			var nsPrefix = GetNsPrefix(_contractInfo.Member);
			var exceptionType = ParseTypeName($"{nsPrefix}InvalidOperationException");

			var statements = new List<StatementSyntax>(_contractInfo.Ensures.Count);
			foreach (var ensure in _contractInfo.Ensures)
			{
				var checkInfo = new CheckInfo(ensure);
				var checkExpr = new ContractResultRewriter(varName).Visit(checkInfo.CheckExpression).NormalizeWhitespace();
				statements.Add(GenerateThrow((ExpressionSyntax) checkExpr, exceptionType, trivias,
					checkInfo.MessageExpression));
			}

			return statements;
		}

		private static ReturnStatementSyntax ReturnResult(TriviaInfo trivias)
		{
			return ReturnStatement()
				.NormalizeWhitespace()
				.WithLeadingTrivia(trivias.IndentedWhitespace)
				.WithTrailingTrivia(trivias.EndOfLine);
		}

		private static ReturnStatementSyntax ReturnResult(string varName, TriviaInfo trivias)
		{
			return ReturnStatement(IdentifierName(varName))
				.NormalizeWhitespace()
				.WithLeadingTrivia(trivias.IndentedWhitespace)
				.WithTrailingTrivia(trivias.EndOfLine);
		}

		private static LocalDeclarationStatementSyntax VarDeclaraion(string varName, ExpressionSyntax initializer,
			TriviaInfo trivias)
		{
			return LocalDeclarationStatement(
					declaration: VariableDeclaration(
						type: IdentifierName("var"),
						variables: SeparatedList(new[]
						{
							VariableDeclarator(identifier: Identifier(varName))
								.WithInitializer(EqualsValueClause(initializer))
						})))
				.NormalizeWhitespace()
				.WithLeadingTrivia(trivias.IndentedWhitespace)
				.WithTrailingTrivia(trivias.EndOfLine);
		}
	}
}
