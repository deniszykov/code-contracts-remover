using JetBrains.Annotations;

namespace Test
{
	public static class StringExtensions
	{
		public static string AsString([NotNull] this string value)
		{
			if (value == null) throw new System.ArgumentNullException(nameof(value), $"Contract assertion not met: {nameof(value)} != null");
			return value;
		}
	}
}
