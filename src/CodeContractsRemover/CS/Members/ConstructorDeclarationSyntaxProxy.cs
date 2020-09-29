using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CodeContractsRemover.CS.Members
{
	class ConstructorDeclarationSyntaxProxy : INodeWithBody
	{
		public ConstructorDeclarationSyntax Node { get; private set; }

		public ConstructorDeclarationSyntaxProxy(ConstructorDeclarationSyntax node)
		{
			Node = node;
			ReturnType = PredefinedType(Token(SyntaxKind.VoidKeyword));
		}

		public BlockSyntax Body
		{
			get => Node.Body;
			set => Node = Node.WithBody(value);
		}

		public TypeSyntax ReturnType { get; }
	}
}
