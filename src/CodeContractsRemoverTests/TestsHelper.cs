using System.IO;
using System.Reflection;

namespace CodeContractsRemoverTests
{
	public class TestsHelper
	{
		public static string GetInitialFileName(string file)
		{
			var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			return Path.Combine(location, file);
		}
	}
}
