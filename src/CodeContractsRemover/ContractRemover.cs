using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CodeContractsRemover.CS;
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
		public const string ContractClassForAttributeName = "ContractClassFor";
		public static readonly HashSet<string> ContractAttributes = new HashSet<string>
		{
			"ContractClass","ContractAbbreviator","ContractArgumentValidator","ContractInvariantMethod","ContractOption","ContractPublicPropertyName","ContractReferenceAssembly",
			"ContractRuntimeIgnored","ContractVerification","Pure",
			"ContractClassAttribute","ContractAbbreviatorAttribute","ContractArgumentValidatorAttribute","ContractInvariantMethodAttribute","ContractOptionAttribute",
			"ContractPublicPropertyNameAttribute","ContractReferenceAssemblyAttribute", "ContractRuntimeIgnoredAttribute","ContractVerificationAttribute", "PureAttribute",
		};

		public static bool Process(string filePath, ContractReplacementMode mode, Encoding encoding)
		{
			if (filePath == null) throw new ArgumentNullException(nameof(filePath));
			if (encoding == null) throw new ArgumentNullException(nameof(encoding));

			Console.Write($"Processing file {Path.GetFileName(filePath)}...");

			var changed = false;
			if (filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
			{
				var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(filePath, encoding), CSharpParseOptions.Default, filePath, encoding);
				if (mode == ContractReplacementMode.Stats)
				{
					var visitor = new CSharpStatsCollector();
					visitor.Visit(tree.GetRoot());
				}
				else
				{
					var visitor = new ContractCSharpSyntaxRewriter(mode);
					var newRoot = visitor.Visit(tree.GetRoot());
					changed = newRoot != tree.GetRoot();
					if (changed)
						File.WriteAllText(filePath, newRoot.ToFullString(), encoding);
				}
			}
			else if (filePath.EndsWith(".vb", StringComparison.OrdinalIgnoreCase))
			{
				var tree = VisualBasicSyntaxTree.ParseText(File.ReadAllText(filePath, encoding), VisualBasicParseOptions.Default, filePath, encoding);
				var visitor = new ContractCSharpSyntaxRewriter(mode);
				var newRoot = visitor.Visit(tree.GetRoot());
				changed = newRoot != tree.GetRoot();
				if (changed)
					File.WriteAllText(filePath, newRoot.ToFullString(), encoding);
			}
			else
			{
				throw new NotSupportedException($"Unknown code file extension {Path.GetExtension(filePath)}. Only .cs and .vb are supported.");
			}

			if (changed)
				Console.WriteLine("Modified.");
			else
				Console.WriteLine("Not Modified.");

			return changed;
		}
	}
}
