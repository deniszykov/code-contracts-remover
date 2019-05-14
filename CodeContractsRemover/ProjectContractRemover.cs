using Microsoft.Build.Evaluation;
using System;
using System.IO;
using System.Linq;

namespace CodeContractsRemover
{
	public static class ProjectContractRemover
	{
		public static void Process(string filePath)
		{
			Console.Write($"Processing file {Path.GetFileName(filePath)}...");

			var changed = false;
			if (filePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) ||
				filePath.EndsWith(".vbproj", StringComparison.OrdinalIgnoreCase))
			{
				var project = new Project(filePath);
				foreach (var pg in project.Xml.PropertyGroups)
				{
					var propsToRemove = pg.Properties.Where(p => p.Name.StartsWith("CodeContracts", StringComparison.OrdinalIgnoreCase)).ToArray();
					foreach (var prop in propsToRemove)
					{
						changed = true;
						pg.RemoveChild(prop);
					}
				}
				if (changed)
				{
					project.Xml.Save();
				}
			}
			else
			{
				throw new NotSupportedException($"Unknown project file extension {Path.GetExtension(filePath)}. Only .csproj and .vbproj are supported.");
			}

			if (changed)
				Console.WriteLine("Modified.");
			else
				Console.WriteLine("Not Modified.");
		}
	}
}
