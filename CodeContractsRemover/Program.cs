using System;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace CodeContractsRemover
{
	[Description("usage code_contracts_remover.exe <Convert|Remove> <directorPath> [searchPattern=*.cs] [encoding=utf-8]")]
	public static class Program
	{
		public static int Main()
		{
			return CommandLine.Run(typeof(Program), CommandLine.Arguments, "Help");
		}

		public static int Remove(string path, string searchPattern = "*.cs", string encoding = "utf-8")
		{

			foreach (var file in Directory.EnumerateFiles(path, searchPattern, SearchOption.AllDirectories))
				ContractRemover.Process(file, ContractReplacementMode.Remove, Encoding.GetEncoding(encoding));
			return 0;
		}

		public static int Convert(string path, string searchPattern = "*.cs", string encoding = "utf-8")
		{
			foreach (var file in Directory.EnumerateFiles(path, searchPattern, SearchOption.AllDirectories))
				ContractRemover.Process(file, ContractReplacementMode.Convert, Encoding.GetEncoding(encoding));

			return 0;
		}

		public static int Help(string commandToDescribe = null)
		{
			return CommandLine.Describe(typeof(Program), commandToDescribe);
		}
	}
}
