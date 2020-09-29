using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using CodeContractsRemover.CS;

namespace CodeContractsRemover
{
	[Description("usage CodeContractsRemover.exe <Convert|Remove|Stats> [--directoryPath .] [--searchPattern *.cs *.csproj] [--encoding utf-8] [--ignorePattern .svn/] [--annotations Add]")]
	public static class Program
	{
		private static readonly string[] DefaultSearchPattern = new[] { "*.cs", "*.vb", "*.csproj", "*.vbproj" };

		public static int Main()
		{
			return CommandLine.Run(typeof(Program), CommandLine.Arguments, "Help");
		}

		public static int Remove(string path = ".", string[] searchPattern = null, string encoding = "utf-8", string[] ignorePattern = null)
		{
			if (searchPattern == null) searchPattern = DefaultSearchPattern;

			foreach (var file in searchPattern.SelectMany(p => Directory.EnumerateFiles(path, p, SearchOption.AllDirectories)))
			{
				if (IsIgnored(file, ignorePattern))
				{
					Console.Write($"Skipping ignored file '{file}'.");
					continue;
				}

				if (file.EndsWith("proj", StringComparison.OrdinalIgnoreCase))
				{
					new ProjectContractRemover(file, AnnotationsMode.None).Process();
				}
				else
				{
					ContractRemover.Process(file, ContractReplacementMode.Remove, Encoding.GetEncoding(encoding));
				}
			}

			return 0;
		}

		public static int Stats(string path = ".", string[] searchPattern = null, string encoding = "utf-8", string[] ignorePattern = null)
		{
			if (searchPattern == null) searchPattern = DefaultSearchPattern;

			foreach (var file in searchPattern.SelectMany(p => Directory.EnumerateFiles(path, p, SearchOption.AllDirectories)))
			{
				if (IsIgnored(file, ignorePattern))
				{
					Console.Write($"Skipping ignored file '{file}'.");
					continue;
				}

				ContractRemover.Process(file, ContractReplacementMode.Stats, Encoding.GetEncoding(encoding));
			}

			Console.WriteLine();
			Console.WriteLine("Statistics");
			Console.WriteLine(CSharpStatsCollector.GetStats());

			return 0;
		}

		public static int Convert(string path = ".", string[] searchPattern = null,
			string encoding = "utf-8", string[] ignorePattern = null,
			AnnotationsMode annotations = AnnotationsMode.IncludeIntoBinaries)
		{
			if (searchPattern == null) searchPattern = DefaultSearchPattern;

			foreach (var file in searchPattern.SelectMany(p => Directory.EnumerateFiles(path, p, SearchOption.AllDirectories)))
			{
				if (IsIgnored(file, ignorePattern))
				{
					Console.Write($"Skipping ignored file '{file}'.");
					continue;
				}

				if (file.EndsWith("proj", StringComparison.OrdinalIgnoreCase))
				{
					new ProjectContractRemover(file, annotations).Process();
				}
				else
				{
					var mode = annotations == AnnotationsMode.None
						? ContractReplacementMode.Convert : ContractReplacementMode.ConvertAndAddAnnotations;
					ContractRemover.Process(file, mode, Encoding.GetEncoding(encoding));
				}
			};

			return 0;
		}

		public static int Help(string commandToDescribe = null)
		{
			return CommandLine.Describe(typeof(Program), commandToDescribe);
		}

		private static bool IsIgnored(string file, string[] ignorePattern)
		{
			if (ignorePattern == null || ignorePattern.Length == 0)
			{
				return false;
			}

			foreach (var pattern in ignorePattern)
			{
				if (file.Replace('/', '\\').IndexOf(pattern.Replace('/', '\\'), StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true; // ignored
				}
			}

			return false;
		}
	}
}
