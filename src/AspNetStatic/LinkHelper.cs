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
			if (htmlContent is null)
			{
				throw new ArgumentNullException(nameof(htmlContent));
			}
			if (pages is null)
			{
				throw new ArgumentNullException(nameof(pages));
			}

			if (string.IsNullOrWhiteSpace(htmlContent)) return htmlContent;
			if (!pages.Any()) return htmlContent;

			var pattern = string.Format(_regex, string.Join('|',
				pages.Select(
					p => p.Route.Equals(RouteConsts.FwdSlash.ToString()) ? p.Route :
					p.Route.EnsureNotStartsWith(RouteConsts.FwdSlash)
					.EnsureNotEndsWith(RouteConsts.FwdSlash))));

			htmlContent = Regex.Replace(
				htmlContent, pattern, m =>
				{
					var href = m.Groups[1].Value;

					var page = pages.FindPage(href, routesAreCaseSensitive);

					if (page is null) return m.Value;

					var newHref =
						!string.IsNullOrWhiteSpace(page.OutFilePathname)
						? page.OutFilePathname
						.Replace(RouteConsts.BakSlash, RouteConsts.FwdSlash)
						.EnsureStartsWith(_fSlash)
						: (page.Route.EndsWith(RouteConsts.FwdSlash) || alwaysDefaultFile)
						? $"{href.EnsureEndsWith(_fSlash)}{defaultFileName}"
						: $"{href.EnsureNotEndsWith(_fSlash)}{pageFileExtension}"
						;

					newHref = href.StartsWith(_fSlash) ? newHref.EnsureStartsWith(_fSlash) : newHref;

					return m.Value.Replace(href, newHref);
				},
				RegexOptions.Compiled
				| RegexOptions.CultureInvariant
				| RegexOptions.IgnoreCase
				| RegexOptions.Multiline
				| RegexOptions.IgnorePatternWhitespace);


			return htmlContent;
		}

		public static PageInfo? FindPage(
			this IEnumerable<PageInfo> pages,
			string href,
			bool routesAreCaseSensitive = default)
		{
			if ((pages is null) || !pages.Any()) return default;

			return
				pages.FirstOrDefault(
					p => href == _fSlash ? p.Url.Equals(_fSlash) :
					p.Url.EnsureNotStartsWith(RouteConsts.FwdSlash)
					.EnsureNotEndsWith(RouteConsts.FwdSlash)
					.Equals(href.EnsureNotStartsWith(RouteConsts.FwdSlash)
					.EnsureNotEndsWith(RouteConsts.FwdSlash),
					routesAreCaseSensitive
					? StringComparison.InvariantCulture
					: StringComparison.InvariantCultureIgnoreCase));
		}

		private static readonly string _regex = @"(?:<a|<area) (?:\s|\w|-|_|=|""|')* (?:\n|\r|\f|\r\n|\n\r|\n\f|\f\n)* (?:\s|\w|-|_|=|""|')* href=[""|']([/]?(?:{0})[/]?)[""|']";

		private static readonly string _fSlash = RouteConsts.FwdSlash.ToString();
	}
}
