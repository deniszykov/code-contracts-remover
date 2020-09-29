using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractsRemover.CS.Members
{
	/// <summary> Info about ctor's, method's or property's contract </summary>
	public class MemberContractInfo
	{
		public MemberContractInfo(MethodDeclarationSyntax node)
		{
			Member = node;
			MemberName = node.Identifier.Text;
			IsStatic = node.IsStatic();
			IsOverride = node.IsOverride();
		}

		public MemberContractInfo(ConstructorDeclarationSyntax node)
		{
			Member = node;
			MemberName = node.Identifier.Text;
			IsStatic = node.IsStatic();
			IsOverride = false;
		}

		public MemberContractInfo(AccessorDeclarationSyntax node)
		{
			Member = node;
			var property = node.Ancestors().OfType<PropertyDeclarationSyntax>().First();
			MemberName = $"{property.Identifier.Text}_{node.Kind()}";
			IsStatic = property.IsStatic();
			IsOverride = property.IsOverride();
		}

		public MemberContractInfo(PropertyDeclarationSyntax node)
		{
			Member = node;
			MemberName = node.Identifier.Text;
			IsStatic = node.IsStatic();
			IsOverride = node.IsOverride();
		}

		public void Init()
		{
			CSharpMemberContractFinder.InitContract(this);
		}

		/// <summary> Member </summary>
		public CSharpSyntaxNode Member { get; set; }

		/// <summary> Requires statements </summary>
		public List<CheckInfo> Requires { get; } = new List<CheckInfo>();

		/// <summary> Ensures statements </summary>
		public List<CheckInfo> Ensures { get; } = new List<CheckInfo>();

		public string MemberName { get; }

		public bool IsStatic { get; }

		public bool IsOverride{ get; }
	}
}
