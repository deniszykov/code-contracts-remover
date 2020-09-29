using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractsRemover.CS.Class
{
	/// <summary> Finds class's contract </summary>
	public class CSharpClassContractFinder : SyntaxWalker
	{
		private readonly ClassContractInfo _contractInfo = new ClassContractInfo();

		private CSharpClassContractFinder(ClassDeclarationSyntax node)
		{
			_contractInfo.Class = node;
		}

		/// <summary>
		/// Called when the walker visits a node.  This method may be overridden if subclasses want
		/// to handle the node.  Overrides should call back into this base method if they want the
		/// children of this node to be visited.
		/// </summary>
		/// <param name="node">The current node that the walker is visiting.</param>
		public override void Visit(SyntaxNode node)
		{
			if (!(node is MethodDeclarationSyntax method))
			{
				base.Visit(node);
				return;
			}
			
			var attrs = method.AttributeLists.SelectMany(a => a.Attributes.Select(aa => aa.Name.ToString())).ToArray();
			if (attrs.Contains("ContractInvariantMethod") || attrs.Contains("ContractInvariantMethodAttribute"))
			{
				if (method.IsStatic())
				{
					_contractInfo.StaticInvariants.Add(method.Identifier.Text);
				}
				else
				{
					_contractInfo.InstanceInvariants.Add(method.Identifier.Text);
				}
			}
			else
			{
				base.Visit(node);
			}
		}

		public static ClassContractInfo GetContract(ClassDeclarationSyntax node)
		{
			if (node == null) throw new ArgumentNullException(nameof(node));

			var finder = new CSharpClassContractFinder(node);
			finder.Visit(node);
			return finder._contractInfo;
		}
	}
}
