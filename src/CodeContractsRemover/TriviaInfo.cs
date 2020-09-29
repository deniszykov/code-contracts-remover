using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharpExtensions;

namespace CodeContractsRemover
{
	public class TriviaInfo
	{
		private const string Indent = "    ";

		public TriviaInfo(SyntaxNode node)
		{
			BaseWhitespace = TriviaList(node.GetLeadingTrivia().Reverse()
				.TakeWhile(stt => stt.IsKind(SyntaxKind.WhitespaceTrivia))
				.Reverse());
			IndentedWhitespace = AddIndent(BaseWhitespace);
			EndOfLine = node.GetTrailingTrivia();
			SpaceList = TriviaList(Space);
		}

		private SyntaxTriviaList AddIndent(SyntaxTriviaList trivia)
		{
			return trivia.Add(Whitespace(Indent));
		}

		private TriviaInfo() { }

		public SyntaxTriviaList BaseWhitespace { get; private set; }
		public SyntaxTriviaList IndentedWhitespace { get; private set; }
		public SyntaxTriviaList EndOfLine { get; private set; }
		public SyntaxTriviaList SpaceList { get; private set; }

		public TriviaInfo AddIndent()
		{
			return new TriviaInfo
			{
				BaseWhitespace = AddIndent(BaseWhitespace),
				IndentedWhitespace = AddIndent(IndentedWhitespace),
				EndOfLine = EndOfLine,
				SpaceList = SpaceList
			};
		}
	}
}
