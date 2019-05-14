using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace CodeContractsRemover
{
	[Description("usage CodeContractsRemover.exe <Convert|Remove> <directoryPath> [--searchPattern *.cs *.csproj] [--encoding utf-8] [--ignorePattern .svn/ ]")]
	public static class Program
	{
		public static int Main()
		{
			return CommandLine.Run(typeof(Program), CommandLine.Arguments, "Help");
		}

		public static int Remove(string path, string[] searchPattern = null, string encoding = "utf-8", string[] ignorePattern = null)
		{
			if (searchPattern == null) searchPattern = new[] { "*.cs", "*.vb" };

			foreach (var file in searchPattern.SelectMany(p => Directory.EnumerateFiles(path, p, SearchOption.AllDirectories)))
			{
				if (IsIgnored(file, ignorePattern))
				{
					Console.Write($"Skipping ignored file '{file}'.");
					continue;
				}

				if (file.EndsWith("proj", StringComparison.OrdinalIgnoreCase))
				{
					ProjectContractRemover.Process(file);
				}
				else
				{
					ContractRemover.Process(file, ContractReplacementMode.Remove, Encoding.GetEncoding(encoding));
				}
			}

			return 0;
		}

		public static int Convert(string path, string[] searchPattern = null, string encoding = "utf-8", string[] ignorePattern = null)
		{
			if (searchPattern == null) searchPattern = new[] { "*.cs", "*.vb" };

			foreach (var file in searchPattern.SelectMany(p => Directory.EnumerateFiles(path, p, SearchOption.AllDirectories)))
			{
				if (IsIgnored(file, ignorePattern))
				{
					Console.Write($"Skipping ignored file '{file}'.");
					continue;
				}

				if (file.EndsWith("proj", StringComparison.OrdinalIgnoreCase))
				{
					ProjectContractRemover.Process(file);
				}
				else
				{
					ContractRemover.Process(file, ContractReplacementMode.Convert, Encoding.GetEncoding(encoding));
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
