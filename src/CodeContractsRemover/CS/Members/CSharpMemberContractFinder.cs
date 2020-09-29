using Microsoft.CodeAnalysis;
using InvocationExpressionSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax;

namespace CodeContractsRemover.CS.Members
{
	/// <summary> Finds ctor's, method's or property's contract </summary>
	public class CSharpMemberContractFinder : SyntaxWalker
	{
		private readonly MemberContractInfo _contractInfo;

		private CSharpMemberContractFinder(MemberContractInfo contractInfo)
		{
			_contractInfo = contractInfo;
		}

		/// <summary>
		/// Called when the walker visits a node.  This method may be overridden if subclasses want
		/// to handle the node.  Overrides should call back into this base method if they want the
		/// children of this node to be visited.
		/// </summary>
		/// <param name="node">The current node that the walker is visiting.</param>
		public override void Visit(SyntaxNode node)
		{
			if (!(node is InvocationExpressionSyntax invNode))
			{
				base.Visit(node);
				return;
			}

			var invInfo = new InvocationInfo(invNode);

			if (invInfo.Class == "Contract" && invInfo.Method == "Ensures")
			{
				_contractInfo.Ensures.Add(new CheckInfo(invNode));
			}
			else if (invInfo.Class == "Contract" && invInfo.Method == "Requires")
			{
				_contractInfo.Requires.Add(new CheckInfo(invNode));
			}
			else
			{
				base.Visit(node);
			}
		}

		public static void InitContract(MemberContractInfo contractInfo)
		{
			var finder = new CSharpMemberContractFinder(contractInfo);
			finder.Visit(contractInfo.Member);
		}
	}
}
