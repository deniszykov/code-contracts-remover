using System.Linq;
using CodeContractsRemover.CS.Members;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CodeContractsRemover.CS
{
	public class CommonRewriterLogic
	{
		private readonly MemberContractInfo _memberContract;

		public CommonRewriterLogic(MemberContractInfo memberContract)
		{
			_memberContract = memberContract;
		}

		public void AddAnnotationAttrs(INodeWithAttrs node, TriviaInfo triviaInfo, SyntaxTriviaList leadingTrivia)
		{
			if (_memberContract.Ensures.Any(r => r.IsNotNullOrEmptyCheck && r.IsResultCheck))
			{
				node.LeadingTrivia = TriviaList(triviaInfo.EndOfLine.Concat(triviaInfo.BaseWhitespace));
				node.AttributeLists = node.AttributeLists.AddAttr("NotNull");
				node.LeadingTrivia = leadingTrivia;
			}
		}
	}
}
