using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractsRemover
{
	public class InvocationInfo
	{
		public InvocationInfo(InvocationExpressionSyntax invNode)
		{
			var accessExpression = invNode.Expression as MemberAccessExpressionSyntax;
			var contractTypeRef = accessExpression?.Expression as IdentifierNameSyntax;
			Method = accessExpression?.Name.Identifier.ValueText ?? "";
			Class = contractTypeRef?.Identifier.ValueText;
			GenericArgs = (accessExpression?.Name as GenericNameSyntax)?.TypeArgumentList.Arguments.ToList();

		}
		public string Class { get; private set; }

		public string Method { get; private set; }

		public List<TypeSyntax> GenericArgs { get; }
	}
}
