using JetBrains.Annotations;

namespace Test
{
	public static class StringExtensions
	{
		public static string AsString([NotNull] this string value)
		{
			if (string.IsNullOrEmpty(value)) throw new System.ArgumentException($"Contract assertion not met: !string.IsNullOrEmpty({nameof(value)})", nameof(value));
			return value;
		}

		public static string AsString2(this string value)
		{
			if (!(string.IsNullOrEmpty(value))) throw new System.ArgumentException($"Contract assertion not met: string.IsNullOrEmpty({nameof(value)})", nameof(value));
			return value;
		}

		[NotNull]
		public static string AsString3(this string value)
		{
		    if (string.IsNullOrEmpty(value)) throw new System.InvalidOperationException($"Contract assertion not met: !string.IsNullOrEmpty($result)");
		    return value;
		}

		public static string AsString4(this string value)
		{
		    if (!(string.IsNullOrEmpty(value))) throw new System.InvalidOperationException($"Contract assertion not met: string.IsNullOrEmpty($result)");
		    return value;
		}
	}
}
