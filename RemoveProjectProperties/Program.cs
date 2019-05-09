using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoveProjectProperties
{
	class Program
	{
		static void Main(string[] args)
		{
			foreach (var csprojpath in Directory.GetFiles(args[0], "*.csproj", SearchOption.AllDirectories))
			{
				if (csprojpath.IndexOf(@"\backup\", StringComparison.InvariantCultureIgnoreCase) >= 0
					|| csprojpath.IndexOf(@"sql", StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					Console.WriteLine("Skipping {0}", Path.GetFileName(csprojpath));
					continue;
				}

				try
				{
					var xf = new Project(csprojpath);
					bool changed = false;
					foreach (var pg in xf.Xml.PropertyGroups)
					{
						var propsToRemove = pg.Properties.Where(p => p.Name.StartsWith("CodeContracts")).ToArray();
						foreach (var prop in propsToRemove)
						{
							changed = true;
							pg.RemoveChild(prop);
						}
					}
					if (changed)
					{
						xf.Xml.Save();
						Console.WriteLine("Updated {0}", Path.GetFileName(csprojpath));
					}
					else
					{
						Console.WriteLine("NOCHNGS {0}", Path.GetFileName(csprojpath));
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("ERROR   {0}", Path.GetFileName(csprojpath));
				}
			}
			Console.WriteLine("Done.");
			Console.ReadLine();
		}
	}
}
