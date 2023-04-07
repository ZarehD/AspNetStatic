/*--------------------------------------------------------------------------------------------------------------------------------
Copyright © 2023 Zareh DerGevorkian

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License. 
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
--------------------------------------------------------------------------------------------------------------------------------*/

namespace AspNetStatic
{
	public static class AspNetStaticExtensions
	{
		public  static string AssureStartsWith(this string? src, char pfx) =>
			(src is null) ? pfx.ToString()
			: src.StartsWith(pfx)
			? src : $"{pfx}{src}";
		public  static string AssureStartsWith(this string? src, string pfx) =>
			(src is null) ? pfx
			: src.StartsWith(pfx, StringComparison.InvariantCultureIgnoreCase)
			? src : $"{pfx}{src}";

		public  static string AssureNotStartsWith(this string? src, char pfx) =>
			(src is null) ? string.Empty
			: src.StartsWith(pfx)
			? src[1..] : src;
		public  static string AssureNotStartsWith(this string? src, string pfx) =>
			(src is null) ? string.Empty
			: src.StartsWith(pfx, StringComparison.InvariantCultureIgnoreCase)
			? src[pfx.Length..] : src;


		public  static string AssureEndsWith(this string? src, char sfx) =>
			(src is null) ? sfx.ToString() :
			src.EndsWith(sfx)
			? src : $"{src}{sfx}";
		public  static string AssureEndsWith(this string? src, string sfx) =>
			(src is null) ? sfx :
			src.EndsWith(sfx, StringComparison.InvariantCultureIgnoreCase)
			? src : $"{src}{sfx}";

		public  static string AssureNotEndsWith(this string? src, char sfx) =>
			(src is null) ? string.Empty :
			src.EndsWith(sfx)
			? src[0..^1] : src;
		public  static string AssureNotEndsWith(this string? src, string sfx) =>
			(src is null) ? string.Empty :
			src.EndsWith(sfx, StringComparison.InvariantCultureIgnoreCase)
			? src[0..^sfx.Length] : src;
	}
}
