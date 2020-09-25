using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharpExtensions;

namespace CodeContractsRemover
{
	public class TriviaInfo
	{
		private const string Indent = "    ";

		public TriviaInfo(SyntaxNode node)
		{
			// this includes comments
			LeadingTrivia = node.GetLeadingTrivia();
			// this is exclusively indention
			BaseWhitespace = SyntaxFactory.TriviaList(LeadingTrivia.Reverse().TakeWhile(stt => CSharpExtensions.IsKind((SyntaxTrivia) stt, SyntaxKind.WhitespaceTrivia))
				.Reverse());
			IndentedWhitespace = AddIndent(BaseWhitespace);
			EndOfLine = node.GetTrailingTrivia();
			SpaceList = SyntaxFactory.TriviaList(SyntaxFactory.Space);
		}

		private SyntaxTriviaList AddIndent(SyntaxTriviaList trivia)
		{
			return trivia.Add(SyntaxFactory.Whitespace(Indent));
		}

		private TriviaInfo() { }

		public SyntaxTriviaList LeadingTrivia { get; private set; }
		public SyntaxTriviaList BaseWhitespace { get; private set; }
		public SyntaxTriviaList IndentedWhitespace { get; private set; }
		public SyntaxTriviaList EndOfLine { get; private set; }
		public SyntaxTriviaList SpaceList { get; private set; }

		public TriviaInfo AddIndent()
		{
			return new TriviaInfo
			{
				LeadingTrivia = AddIndent(LeadingTrivia),
				BaseWhitespace = AddIndent(BaseWhitespace),
				IndentedWhitespace = AddIndent(IndentedWhitespace),
				EndOfLine = EndOfLine,
				SpaceList = SpaceList
			};
		}
	}
}
