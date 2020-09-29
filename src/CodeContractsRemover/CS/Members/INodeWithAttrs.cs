using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractsRemover.CS.Members
{
	public interface INodeWithAttrs : INode
	{
		SyntaxList<AttributeListSyntax> AttributeLists { get; set; }

	}
}
