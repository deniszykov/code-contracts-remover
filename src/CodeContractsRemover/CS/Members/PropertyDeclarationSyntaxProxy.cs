using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractsRemover.CS.Members
{
	class PropertyDeclarationSyntaxProxy : INodeWithAttrs
	{
		public PropertyDeclarationSyntax Node { get; private set; }

		public PropertyDeclarationSyntaxProxy(PropertyDeclarationSyntax node)
		{
			Node = node;
		}

		public SyntaxList<AttributeListSyntax> AttributeLists
		{
			get => Node.AttributeLists;
			set => Node = Node.WithAttributeLists(value);
		}

		public SyntaxTriviaList LeadingTrivia
		{
			get => Node.GetLeadingTrivia();
			set => Node = Node.WithLeadingTrivia(value);
		}
	}
}
