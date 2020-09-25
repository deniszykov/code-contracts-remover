using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractsRemover
{
	/// <summary> Info about member's contract </summary>
	public class ContractInfo
	{
		/// <summary> Member </summary>
		public MethodDeclarationSyntax Member { get; set; }

		/// <summary> Requires statements </summary>
		public List<InvocationExpressionSyntax> Requires { get; } = new List<InvocationExpressionSyntax>();

		/// <summary> Ensures statements </summary>
		public List<InvocationExpressionSyntax> Ensures { get; } = new List<InvocationExpressionSyntax>();

		/// <summary> Indicates whether member has contract </summary>
		public bool HasContract => Requires.Any() || Ensures.Any();
	}
}
