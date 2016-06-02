using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;

namespace CodeContractsRemover
{
	public class ContractRemover
	{
		public static readonly HashSet<string> ContractMethods = new HashSet<string>
		{
			"Assert","Assume","EndContractBlock","Ensures","EnsuresOnThrow","Exists","ForAll",
			"Invariant","OldValue","Requires","Result","ValueAtReturn"
		};

		public static void Process(string filePath, ContractReplacementMode mode, Encoding encoding)
		{
			if (filePath == null) throw new ArgumentNullException(nameof(filePath));
			if (encoding == null) throw new ArgumentNullException(nameof(encoding));

			Console.Write($"Processing file {Path.GetFileName(filePath)}...");

			var changed = false;
			if (filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
			{
				var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(filePath, encoding), CSharpParseOptions.Default, filePath, encoding);
				var visitor = new ContractCSharpSyntaxRewriter(mode);
				var newRoot = visitor.Visit(tree.GetRoot());
				changed = newRoot != tree.GetRoot();
				if (changed)
					File.WriteAllText(filePath, newRoot.ToString(), encoding);

			}
			else if (filePath.EndsWith(".vb", StringComparison.OrdinalIgnoreCase))
			{
				var tree = VisualBasicSyntaxTree.ParseText(File.ReadAllText(filePath, encoding), VisualBasicParseOptions.Default, filePath, encoding);
				var visitor = new ContractCSharpSyntaxRewriter(mode);
				var newRoot = visitor.Visit(tree.GetRoot());
				changed = newRoot != tree.GetRoot();
				if (changed)
					File.WriteAllText(filePath, newRoot.ToString(), encoding);
			}
			else
			{
				throw new NotSupportedException($"Unknown file extention {Path.GetExtension(filePath)}. Only .cs and .vb is supported.");
			}

			if (changed)
				Console.WriteLine("Modified.");
			else
				Console.WriteLine("Not Modified.");
		}
	}
}
