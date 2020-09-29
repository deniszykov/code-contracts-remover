using System.Diagnostics.Contracts;

namespace Test
{
	public static class StringExtensions
	{
		public static string AsString(this string value)
		{
			Contract.Requires(!string.IsNullOrEmpty(value));
			return value;
		}

		public static string AsString2(this string value)
		{
			Contract.Requires(string.IsNullOrEmpty(value));
			return value;
		}

		public static string AsString3(this string value)
		{
			Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
			return value;
		}

		public static string AsString4(this string value)
		{
			Contract.Ensures(string.IsNullOrEmpty(Contract.Result<string>()));
			return value;
		}
	}
}
