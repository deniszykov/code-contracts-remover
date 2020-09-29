using System;
using System.Collections.Generic;
using System.Linq;
using CodeContractsRemover.CS.Class;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CodeContractsRemover.CS.Members
{
	/// <summary> Rewrites ctors, methods and props </summary>
	public class CSharpMemberRewriter : CSharpSyntaxRewriter
	{
		private readonly MemberContractInfo _memberContract;
		private ReturnStatementSyntax _ignoreReturn = null;
		private readonly ClassContractInfo _classContract;
		private readonly ContractReplacementMode _mode;

		private CSharpMemberRewriter(MemberContractInfo memberContract, ClassContractInfo classContract,
			ContractReplacementMode mode)
		{
			var modes = new[] { ContractReplacementMode.Convert, ContractReplacementMode.ConvertAndAddAnnotations };
			if (!modes.Contains(mode))
				throw new ArgumentOutOfRangeException(nameof(mode), mode, $"Use only {string.Join(", ", modes)}");

			memberContract.Init();
			_memberContract = memberContract;
			_classContract = classContract;
			_mode = mode;
		}

		#region Public contract

		public static SyntaxNode Visit(MethodDeclarationSyntax node, ClassContractInfo classContract,
			ContractReplacementMode mode)
		{
			var contractInfo = new MemberContractInfo(node);
			return new CSharpMemberRewriter(contractInfo, classContract, mode).Visit(node);
		}

		public static SyntaxNode Visit(ConstructorDeclarationSyntax node, ClassContractInfo classContract,
			ContractReplacementMode mode)
		{
			var contractInfo = new MemberContractInfo(node);
			return new CSharpMemberRewriter(contractInfo, classContract, mode).Visit(node);
		}

		public static SyntaxNode Visit(AccessorDeclarationSyntax node, ClassContractInfo classContract,
			ContractReplacementMode mode)
		{
			var contractInfo = new MemberContractInfo(node);
			return new CSharpMemberRewriter(contractInfo, classContract, mode).Visit(node);
		}

		#endregion

		#region Rewriting logic

		/// <summary>Called when the visitor visits a MethodDeclarationSyntax node.</summary>
		public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
		{
			node = RemoveContractAttrs(node);
			var proxy = new MethodDeclarationSyntaxProxy(node);
			if (_mode == ContractReplacementMode.ConvertAndAddAnnotations)
			{
				new CommonRewriterLogic(_memberContract)
					.AddAnnotationAttrs(proxy, new TriviaInfo(node), node.GetLeadingTrivia());
			}

			ProcessBody(proxy);

			return base.VisitMethodDeclaration(proxy.Node);
		}

		/// <summary>Called when the visitor visits a ConstructorDeclarationSyntax node.</summary>
		public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
		{
			node = RemoveContractAttrs(node);
			var proxy = new ConstructorDeclarationSyntaxProxy(node);
			ProcessBody(proxy);

			return base.VisitConstructorDeclaration(proxy.Node);
		}

		/// <summary>Called when the visitor visits a AccessorDeclarationSyntax node.</summary>
		public override SyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
		{
			var proxy = new AccessorDeclarationSyntaxProxy(node);
			ProcessBody(proxy);

			return base.VisitAccessorDeclaration(proxy.Node);
		}

		private void ProcessBody(INodeWithBody node)
		{
			// Convert Requires to exception at the beginning of method
			if (_memberContract.Requires.Any())
			{
				var reqStatements = new List<StatementSyntax>(_memberContract.Requires.Count);
				foreach (var require in _memberContract.Requires)
				{
					reqStatements.Add(GenerateArgException(require));
				}

				var statements = node.Body.Statements.InsertRange(0, reqStatements);
				node.Body = node.Body.WithStatements(new SyntaxList<StatementSyntax>(statements));
			}

			var returnType = (node.ReturnType as PredefinedTypeSyntax)?.Keyword;
			if (NeedEnsuresBlock)
			{
				// If method returns void, add ensures and invariants at the end of method
				if (returnType != null && returnType.Value.IsKind(SyntaxKind.VoidKeyword))
				{
					var trivias = new TriviaInfo(node.Body).AddIndent();
					var ensStatements = GenerateEnsures(String.Empty, trivias);

					var statements = node.Body.Statements.AddRange(ensStatements);
					node.Body = node.Body.WithStatements(new SyntaxList<StatementSyntax>(statements));
				}
				// If method has only one return, we'll rewrite it here to avoid adding unnecessary brackets
				else
				{
					var returns = node.Body.Statements.OfType<ReturnStatementSyntax>().ToArray();
					if (returns.Length == 1)
					{
						var ensStatements = GenerateEnsures(returns[0], new TriviaInfo(node.Body).AddIndent());

						var statements = node.Body.Statements
							.Remove(returns[0])
							.AddRange(ensStatements);
						node.Body = node.Body.WithStatements(new SyntaxList<StatementSyntax>(statements));

						_ignoreReturn = node.Body.Statements.OfType<ReturnStatementSyntax>().Single();
					}
				}
			}
		}

		private static T RemoveContractAttrs<T>(T node)
			where T : BaseMethodDeclarationSyntax
		{
			var lists = new List<AttributeListSyntax>(node.AttributeLists.Count);
			SyntaxTriviaList? trivia = null;
			foreach (var attrList in node.AttributeLists.ToArray())
			{
				var list = attrList;
				if (trivia != null)
				{
					list = list.WithLeadingTrivia(list.GetLeadingTrivia().AddRange(trivia.Value));
					trivia = null;
				}

				list = RemoveContractAttrs(list);

				if (list.Attributes.Any())
				{
					lists.Add(list);
				}
				else
				{
					// Relocate leading trivia to next node
					trivia = list.GetLeadingTrivia();
				}
			}

			if (node.AttributeLists.ToArray().SequenceEqual(lists))
			{
				return node;
			}

			node = (T) node.WithAttributeLists(List(lists));
			if (trivia != null)
			{
				node = node.WithLeadingTrivia(node.GetLeadingTrivia().AddRange(trivia.Value));
			}

			return node;
		}

		private static AttributeListSyntax RemoveContractAttrs(AttributeListSyntax list)
		{
			var toRemove = new List<AttributeSyntax>();
			var attrs = list.Attributes;
			foreach (var attr in attrs)
			{
				if (attr.Name.ToString().StartsWith("ContractInvariantMethod"))
				{
					toRemove.Add(attr);
				}
			}

			if (!toRemove.Any())
			{
				return list;
			}

			foreach (var attr in toRemove)
			{
				attrs = attrs.Remove(attr);
			}

			return list.WithAttributes(attrs);
		}


		public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
		{
			var invNode = node.Expression as InvocationExpressionSyntax;
			if (invNode == null)
			{
				return base.VisitExpressionStatement(node);
			}

			var invInfo = new InvocationInfo(invNode);
			var exceptionMethods = new[] {"Assert", "Assume", "Invariant"};
			if (invInfo.Class == "Contract" && exceptionMethods.Contains(invInfo.Method))
			{
				return GenerateArgException(new CheckInfo(invNode));
			}

			var removeMethods = new[] { "Requires", "Ensures" };
			if (invInfo.Class == "Contract" && removeMethods.Contains(invInfo.Method))
			{
				// All requires have been added to the beginning of method in VisitMethodDeclaration
				// Ensures are rewrited in VisitMethodDeclaration and VisitReturn
				return null;
			}

			return base.VisitExpressionStatement(node);
		}

		/// <summary>Called when the visitor visits a ReturnStatementSyntax node.</summary>
		public override SyntaxNode VisitReturnStatement(ReturnStatementSyntax node)
		{
			if(node == _ignoreReturn)
			{
				return base.VisitReturnStatement(node);

			}

			if (NeedEnsuresBlock)
			{
				var trivias = new TriviaInfo(node.Parent is BlockSyntax ? node : node.Parent);

				return Block(GenerateEnsures(node, trivias.AddIndent()))
					.WithOpenBraceToken(Token(trivias.BaseWhitespace, SyntaxKind.OpenBraceToken, trivias.EndOfLine))
					.WithCloseBraceToken(Token(trivias.BaseWhitespace, SyntaxKind.CloseBraceToken, trivias.EndOfLine));
			}

			return base.VisitReturnStatement(node);
		}

		/// <summary>Called when the visitor visits a ParameterSyntax node.</summary>
		public override SyntaxNode VisitParameter(ParameterSyntax node)
		{
			var paramChecks = _memberContract.Requires.Where(r => r.IdName == node.Identifier.ValueText).ToArray();
			if (paramChecks.Any(c => c.IsNotNullCheck))
			{
				var attributeLists = node.AttributeLists.AddAttr("NotNull");
				node = node.WithAttributeLists(attributeLists).NormalizeWhitespace();
			}

			return base.VisitParameter(node);
		}

		#endregion

		#region Requires, assert, assume

		private StatementSyntax GenerateArgException(CheckInfo checkInfo)
		{
			var nsPrefix = GetNsPrefix(_memberContract.Member);
			var parameters = GetParameters(_memberContract.Member);
			var exceptionType = checkInfo.ExceptionType ??
			                    (
				                    checkInfo.IsNullCheck
					                    ? ParseTypeName(nsPrefix + "ArgumentNullException")
					                    : checkInfo.IsRangeCheck
						                    ? ParseTypeName(nsPrefix + "ArgumentOutOfRangeException")
						                    : ParseTypeName(nsPrefix + "ArgumentException")
			                    );

			var paramNameRef = parameters.Contains(checkInfo.IdName)
				? (ExpressionSyntax) InvocationExpression(IdentifierName("nameof"))
					.WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(checkInfo.Id))))
				: LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(checkInfo.IdName ?? "value"));

			var exceptionParams = new[] {paramNameRef, checkInfo.Message};

			if (exceptionType.ToString().EndsWith(".ArgumentException", StringComparison.Ordinal) ||
			    exceptionType.ToString().Equals("ArgumentException", StringComparison.Ordinal))
			{
				Array.Reverse(exceptionParams); // ArgumentException has reverse params order
			}

			return GenerateThrow(checkInfo.Condition, exceptionType, new TriviaInfo(checkInfo.Invocation.Parent),
				exceptionParams);
		}

		#endregion

		#region Ensures and invariants

		private bool NeedEnsuresBlock => _memberContract.Ensures.Any()
		                                 || NeedInvariantBlock && Invariants.Any();

		private List<string> Invariants => _memberContract.IsStatic ? _classContract.StaticInvariants : _classContract.InstanceInvariants;

		private bool NeedInvariantBlock =>
			// ccrewrite doesnt check invariants in overrides
			!_memberContract.IsOverride
			// invariant checks shouldn't invoke invariant checks
			&& !_classContract.AllInvariants.Contains(_memberContract.MemberName);

		private List<StatementSyntax> GenerateEnsures(ReturnStatementSyntax node, TriviaInfo trivias)
		{
			var returnsId = node.Expression?.IsKind(SyntaxKind.IdentifierName) ?? false;
			var varName = returnsId
				? ((IdentifierNameSyntax) node.Expression).Identifier.Text
				: $"result";

			var statements = new List<StatementSyntax>(_memberContract.Ensures.Count + 2);
			var isVoidMethod = node.Expression == null;
			var leadingTrivia = node.GetLeadingTrivia().Reverse()
				.SkipWhile(t => t.IsKind(SyntaxKind.WhitespaceTrivia))
				.Reverse()
				.Union(trivias.BaseWhitespace)
				.ToArray();

			if (!isVoidMethod && !returnsId)
			{
				statements.Add(VarDeclaraion(varName, node.Expression, trivias, TriviaList(leadingTrivia)));
			}

			statements.AddRange(GenerateEnsures(varName, trivias));

			if (returnsId)
			{
				statements[0] = statements[0].WithLeadingTrivia(leadingTrivia);
			}

			if (isVoidMethod)
			{
				statements.Add(ReturnResult(trivias));
			}
			else
			{
				statements.Add(ReturnResult(varName, trivias));
			}

			return statements;
		}

		private List<StatementSyntax> GenerateEnsures(string varName, TriviaInfo trivias)
		{
			var nsPrefix = GetNsPrefix(_memberContract.Member);
			var exceptionType = ParseTypeName($"{nsPrefix}InvalidOperationException");

			var statements = new List<StatementSyntax>(_memberContract.Ensures.Count
			                                           + (NeedInvariantBlock ? Invariants.Count : 0));
			foreach (var ensure in _memberContract.Ensures)
			{
				var checkExpr = new ContractResultRewriter(varName).Visit(ensure.Condition).NormalizeWhitespace();
				statements.Add(GenerateThrow((ExpressionSyntax) checkExpr, exceptionType, trivias,
					ensure.Message));
			}

			if (NeedInvariantBlock)
			{
				foreach (var method in Invariants)
				{
					statements.Add(GenerateInvariantCall(method, trivias));
				}
			}

			return statements;
		}

		private static ExpressionStatementSyntax GenerateInvariantCall(string method, TriviaInfo trivias)
		{
			return ExpressionStatement(InvocationExpression(IdentifierName(method)))
				.NormalizeWhitespace()
				.WithLeadingTrivia(trivias.BaseWhitespace)
				.WithTrailingTrivia(trivias.EndOfLine);
		}

		#endregion

		#region Helpers

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
					ThrowStatement(
							ObjectCreationExpression(exceptionType, ArgumentList(exArgs), null))
						.NormalizeWhitespace()
						.WithSemicolonToken(Token(TriviaList(), SyntaxKind.SemicolonToken, trivias.EndOfLine)))
				.WithIfKeyword(Token(trivias.BaseWhitespace, SyntaxKind.IfKeyword, trivias.SpaceList))
				.WithCloseParenToken(Token(TriviaList(), SyntaxKind.CloseParenToken, trivias.SpaceList));
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


		private static List<string> GetParameters(SyntaxNode node)
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
			return this.HasNamespace("System", node.SyntaxTree) ? "" : "System.";
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

		private static ReturnStatementSyntax ReturnResult(TriviaInfo trivias)
		{
			return ReturnStatement()
				.NormalizeWhitespace()
				.WithLeadingTrivia(trivias.BaseWhitespace)
				.WithTrailingTrivia(trivias.EndOfLine);
		}

		private static ReturnStatementSyntax ReturnResult(string varName, TriviaInfo trivias)
		{
			return ReturnStatement(IdentifierName(varName))
				.NormalizeWhitespace()
				.WithLeadingTrivia(trivias.BaseWhitespace)
				.WithTrailingTrivia(trivias.EndOfLine);
		}

		private static LocalDeclarationStatementSyntax VarDeclaraion(string varName, ExpressionSyntax initializer,
			TriviaInfo trivias, SyntaxTriviaList leadingTrivia)
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
				.WithLeadingTrivia(leadingTrivia)
				.WithTrailingTrivia(trivias.EndOfLine);
		}

		#endregion
	}
}
