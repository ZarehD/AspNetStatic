/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023 Zareh DerGevorkian

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License. 
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
--------------------------------------------------------------------------------------------------------------------------------*/

using System.Text.RegularExpressions;

namespace AspNetStatic
{
	internal static class LinkHelper
	{
		public static string FixupHrefValues(
			this string htmlContent,
			IEnumerable<PageInfo> pages,
			string defaultFileName,
			string pageFileExtension,
			bool alwaysDefaultFile = default,
			bool routesAreCaseSensitive = default)
		{
			Throw.IfNull(htmlContent, nameof(htmlContent));
			Throw.IfNull(pages, nameof(pages));

			if (string.IsNullOrWhiteSpace(htmlContent)) return htmlContent;
			if (!pages.Any()) return htmlContent;

			var pattern = string.Format(_regex, string.Join('|',
				pages.Select(
					p => p.Url.Equals(Consts.FSlash) ? p.Url :
					p.Url.EnsureNotStartsWith(Consts.FwdSlash)
					.EnsureNotEndsWith(Consts.FwdSlash)
					.Replace("?", "\\?"))));

			htmlContent = Regex.Replace(
				htmlContent, pattern, m =>
				{
					var href = m.Groups[1].Value;

					var page = pages.GetPageForUrl(href, routesAreCaseSensitive);

					if (page is null) return m.Value;

					var newHref =
						!string.IsNullOrWhiteSpace(page.OutFilePathname)
						? page.OutFilePathname.Replace(Consts.BakSlash, Consts.FwdSlash).EnsureStartsWith(Consts.FSlash)
						: (page.Route.EndsWith(Consts.FwdSlash) || alwaysDefaultFile)
						? $"{page.Route.EnsureEndsWith(Consts.FSlash)}{defaultFileName}"
						: $"{page.Route.EnsureNotEndsWith(Consts.FSlash)}{pageFileExtension}"
						;

					newHref = href.StartsWith(Consts.FwdSlash)
						? newHref.EnsureStartsWith(Consts.FSlash)
						: newHref.EnsureNotStartsWith(Consts.FSlash);

					return m.Value.Replace(href, newHref);
				},
				RegexOptions.Compiled
				| RegexOptions.CultureInvariant
				| RegexOptions.IgnoreCase
				| RegexOptions.Multiline
				| RegexOptions.IgnorePatternWhitespace);


			return htmlContent;
		}

		private static readonly string _regex = @"(?:<a|<area) (?:\s|\w|-|_|=|""|')* (?:\n|\r|\f|\r\n|\n\r|\n\f|\f\n)* (?:\s|\w|-|_|=|""|')* href=[""|']([/]?(?:{0})[/]?)[""|']";
	}
}
