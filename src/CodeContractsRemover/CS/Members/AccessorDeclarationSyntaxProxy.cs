using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CodeContractsRemover.CS.Members
{
	class AccessorDeclarationSyntaxProxy : INodeWithBody
	{
		public AccessorDeclarationSyntax Node { get; private set; }

		public AccessorDeclarationSyntaxProxy(AccessorDeclarationSyntax node)
		{
			Node = node;
			if (node.IsKind(SyntaxKind.GetAccessorDeclaration))
			{
				var property = node.Ancestors().OfType<PropertyDeclarationSyntax>().First();
				ReturnType = property.Type;
			}
			else
			{
				ReturnType = PredefinedType(Token(SyntaxKind.VoidKeyword));
			}

		}

		public BlockSyntax Body
		{
			get => Node.Body;
			set => Node = Node.WithBody(value);
		}

		public TypeSyntax ReturnType { get; private set; }
	}
}
