using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractsRemover.CS.Members
{
	class MethodDeclarationSyntaxProxy : INodeWithBody, INodeWithAttrs
	{
		public MethodDeclarationSyntax Node { get; private set; }

		public MethodDeclarationSyntaxProxy(MethodDeclarationSyntax node)
		{
			Node = node;
		}

		public BlockSyntax Body
		{
			get => Node.Body;
			set => Node = Node.WithBody(value);
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

		public TypeSyntax ReturnType => Node.ReturnType;
	}
}
