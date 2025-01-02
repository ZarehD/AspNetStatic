/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023-2025 Zareh DerGevorkian

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License. 
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
--------------------------------------------------------------------------------------------------------------------------------*/

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace AspNetStatic.OBSOLETE
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
		public static T? IfNull<T>(bool condition, T? arg, [CallerArgumentExpression("arg")] string? argName = default)
		{
			if (condition && (arg is null))
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

		public static void IfTrue<TException>(bool condition, string? message = default) where TException : Exception
		{
			if (condition)
			{
				throw (TException) new Exception(message);
			}
		}

		[DoesNotReturn]
		public static void NullArg(string? argName = default) => throw new ArgumentNullException(argName);

		[DoesNotReturn]
		public static void BadArg(string? argName = default, string? message = default, params string[] args) =>
			throw new ArgumentException(
				string.Format(CultureInfo.InvariantCulture, message ?? string.Empty, args),
				argName);

		[DoesNotReturn]
		public static void InvalidOp(string? message = default, params string[] args) =>
			throw new InvalidOperationException(
				string.Format(CultureInfo.InvariantCulture, message ?? string.Empty, args));
	}
}
