using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace AspNetStatic
{
	[StackTraceHidden]
	internal static class Throw
	{
		public static T IfNull<T>([NotNull] T? arg, [CallerArgumentExpression("arg")] string? argName = default)
		{
			if (arg is null)
			{
				NullArg(argName);
			}

			return arg;
		}
		public static string IfNullOrEmpty([NotNull] string? arg, [CallerArgumentExpression("arg")] string? argName = default, string? message = default, params string[] args)
		{
			if (string.IsNullOrEmpty(arg))
			{
				BadArg(argName, message, args);
			}

			return arg;
		}
		public static string IfNullOrWhiteSpace([NotNull] string? arg, [CallerArgumentExpression("arg")] string? argName = default, string? message = default, params string[] args)
		{
			if (string.IsNullOrWhiteSpace(arg))
			{
				BadArg(argName, message, args);
			}

			return arg;
		}

		[DoesNotReturn]
		public static void NullArg(string? argName = default) => throw new ArgumentNullException(argName);

		[DoesNotReturn]
		public static void BadArg(string? argName = default, string? message = default, params string[] args) =>
			throw new ArgumentException(
				string.Format(CultureInfo.InvariantCulture, message, args),
				argName);

		[DoesNotReturn]
		public static void InvalidOp(string? message = default, params string[] args) =>
			throw new InvalidOperationException(
				string.Format(CultureInfo.InvariantCulture, message ?? string.Empty, args));
	}
}
