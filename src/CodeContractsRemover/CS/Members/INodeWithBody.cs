using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractsRemover.CS.Members
{
	public interface INodeWithBody
	{
		BlockSyntax Body { get; set; }

		TypeSyntax ReturnType { get; }
	}
}
