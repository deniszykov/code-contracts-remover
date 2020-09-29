using System.Diagnostics.Contracts;

namespace Test
{
	public static class StringExtensions
	{
		public static string AsString(this string value)
		{
			Contract.Requires(value != null);
			return value;
		}
	}
}
