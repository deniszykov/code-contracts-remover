using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractsRemover.CS
{
	public class InvocationInfo
	{
		public InvocationInfo(InvocationExpressionSyntax invNode)
		{
			var accessExpression = invNode.Expression as MemberAccessExpressionSyntax;
			var contractTypeRef = accessExpression?.Expression as IdentifierNameSyntax;
			Method = accessExpression?.Name.Identifier.ValueText ?? "";
			Class = contractTypeRef?.Identifier.ValueText;
		}

		public string Class { get; private set; }

		public string Method { get; private set; }
	}
}
