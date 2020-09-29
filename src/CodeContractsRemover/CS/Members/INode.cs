using Microsoft.CodeAnalysis;

namespace CodeContractsRemover.CS.Members
{
	public interface INode
	{
		SyntaxTriviaList LeadingTrivia { get; set; }

	}
}
