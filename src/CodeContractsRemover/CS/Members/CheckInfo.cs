using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractsRemover.CS.Members
{
	public class CheckInfo
	{
		public CheckInfo(InvocationExpressionSyntax node)
		{
			Invocation = node;
			var accessExpression = node.Expression as MemberAccessExpressionSyntax;
			ExceptionType = (accessExpression?.Name as GenericNameSyntax)?.TypeArgumentList.Arguments.FirstOrDefault();

			Condition = node.ArgumentList.Arguments[0].Expression;
			Message = node.ArgumentList.Arguments.Count > 1
				? node.ArgumentList.Arguments[1].Expression
				: SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
					SyntaxFactory.Literal("Contract assertion not met: " + Condition));
			var firstId = Condition.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();
			if (firstId?.Identifier.ValueText == nameof(string.IsNullOrEmpty))
			{
				_isNotNullOrEmptyCheck = Condition.IsKind(SyntaxKind.LogicalNotExpression);
				Id = Condition.DescendantNodes().OfType<IdentifierNameSyntax>().Except(new []{firstId}).FirstOrDefault();
			}
			else
			{
				_isNotNullOrEmptyCheck = false;
				Id = firstId;
			}

			IsResultCheck = Condition.DescendantNodes().OfType<InvocationExpressionSyntax>()
				.Any(i =>
				{
					var info = new InvocationInfo(i);
					return info.Class == "Contract" && info.Method == "Result";
				});

			if (Condition is BinaryExpressionSyntax binaryExpression)
			{
				OperatorKind = binaryExpression.OperatorToken.Kind();
			}
		}

		public bool IsResultCheck { get; }

		/// <summary> Exception to be thrown if the condition is not met </summary>
		public TypeSyntax ExceptionType { get; set; }

		/// <summary> Initial invocation </summary>
		public InvocationExpressionSyntax Invocation { get; }

		/// <summary> Check expression, e.g. arg != null </summary>
		public ExpressionSyntax Condition { get; }

		/// <summary> Message to throw if the requirement is not met </summary>
		public ExpressionSyntax Message { get; }

		/// <summary> Id that is checked </summary>
		public IdentifierNameSyntax Id { get; }

		/// <summary> Id's name </summary>
		public string IdName => Id?.Identifier.ValueText;

		/// <summary> Operator in binary expression </summary>
		public SyntaxKind OperatorKind { get; }

		public bool IsNullCheck => Condition.IsNullCheck();

		private readonly bool _isNotNullOrEmptyCheck;

		public bool IsNotNullOrEmptyCheck =>
			_isNotNullOrEmptyCheck || IsNullCheck && Condition.IsKind(SyntaxKind.NotEqualsExpression);

		public bool IsRangeCheck => Condition.IsRangeCheck();

		/// <summary>Returns a string that represents the current object.</summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString()
		{
			return Invocation.ToString();
		}
	}
}
