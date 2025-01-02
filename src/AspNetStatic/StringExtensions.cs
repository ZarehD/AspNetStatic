/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023-2025 Zareh DerGevorkian

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License. 
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
--------------------------------------------------------------------------------------------------------------------------------*/

namespace AspNetStatic
{
	internal static class StringExtensions
	{
		public static bool HasSameText(this string? src, string? other,
			StringComparison comparisonMode = StringComparison.OrdinalIgnoreCase) =>
			((src is null) && (other is null)) || (src?.Equals(other, comparisonMode) ?? false);


		public static string EnsureStartsWith(this string? src, char pfx, bool onlyIfHasValue = default) =>
			string.IsNullOrEmpty(src) ? onlyIfHasValue ? string.Empty : pfx.ToString()
			: src.StartsWith(pfx)
			? src : $"{pfx}{src}";
		public static string EnsureStartsWith(this string? src, string pfx, bool onlyIfHasValue = default) =>
			string.IsNullOrEmpty(src) ? onlyIfHasValue ? string.Empty : pfx
			: src.StartsWith(pfx, StringComparison.OrdinalIgnoreCase)
			? src : $"{pfx}{src}";

		public static string EnsureNotStartsWith(this string? src, char pfx) =>
			(src is null) ? string.Empty
			: src.StartsWith(pfx)
			? src[1..] : src;
		public static string EnsureNotStartsWith(this string? src, string pfx) =>
			(src is null) ? string.Empty
			: src.StartsWith(pfx, StringComparison.OrdinalIgnoreCase)
			? src[pfx.Length..] : src;


		public static string EnsureEndsWith(this string? src, char sfx, bool onlyIfHasValue = default) =>
			string.IsNullOrEmpty(src) ? onlyIfHasValue ? string.Empty : sfx.ToString() :
			src.EndsWith(sfx)
			? src : $"{src}{sfx}";
		public static string EnsureEndsWith(this string? src, string sfx, bool onlyIfHasValue = default) =>
			string.IsNullOrEmpty(src) ? onlyIfHasValue ? string.Empty : sfx :
			src.EndsWith(sfx, StringComparison.OrdinalIgnoreCase)
			? src : $"{src}{sfx}";

		public static string EnsureNotEndsWith(this string? src, char sfx) =>
			(src is null) ? string.Empty :
			src.EndsWith(sfx)
			? src[0..^1] : src;
		public static string EnsureNotEndsWith(this string? src, string sfx) =>
			(src is null) ? string.Empty :
			src.EndsWith(sfx, StringComparison.OrdinalIgnoreCase)
			? src[0..^sfx.Length] : src;
	}
}
