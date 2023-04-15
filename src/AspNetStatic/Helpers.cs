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
	internal static class Helpers
	{
		public static string ToDefaultFileFallback(
			this string route, string[] exclusions,
			string defaultFileName, string pageFileExtension)
		{
			Throw.IfNullOrWhiteSpace(route, nameof(route), Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace);
			Throw.IfNullOrWhiteSpace(defaultFileName, nameof(defaultFileName), Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace);
			Throw.IfNullOrWhiteSpace(pageFileExtension, nameof(pageFileExtension), Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace);

			return
				((exclusions is null) || !exclusions.Any(
					x => route.EnsureNotEndsWith(Consts.FwdSlash)?
					.EndsWith(x, StringComparison.OrdinalIgnoreCase) ?? false))
				? route.EnsureEndsWith(Consts.FwdSlash) + defaultFileName
				: route.EnsureNotEndsWith(Consts.FwdSlash) + pageFileExtension
				;
		}

		public static string GetOutFilePathname(
			this PageInfo page,
			string rootFolder,
			bool alwaysCreateDefaultFile,
			string indexFileName,
			string pageFileExtension,
			string[] exclusions)
		{
			Throw.IfNull(page, nameof(page));
			Throw.IfNullOrWhiteSpace(
				rootFolder, nameof(rootFolder),
				Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace);

			var pagePath = string.Empty;

			if (!string.IsNullOrWhiteSpace(page.OutFile))
			{
				pagePath = page.OutFile;
			}
			else
			{
				Throw.IfNullOrWhiteSpace(
					indexFileName, nameof(indexFileName),
					Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace);

				Throw.IfNullOrWhiteSpace(
					pageFileExtension, nameof(pageFileExtension),
					Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace);

				exclusions ??= Array.Empty<string>();

				var pageRoute = page.Route.StripQueryString();
				var uriKind = UriKind.Relative;

				if (!Uri.IsWellFormedUriString(pageRoute, uriKind))
				{
					Throw.InvalidOp(Properties.Resources.Err_RouteForPageNotWellFormed, uriKind.ToString());
				}
				pagePath = pageRoute;

				if (pagePath.EndsWith(Consts.FwdSlash) || pagePath.EndsWith(Consts.BakSlash))
				{
					pagePath += indexFileName;
				}
				else if (!Path.HasExtension(pagePath)) //(string.IsNullOrEmpty(Path.GetExtension(pagePath)))
				{
					var generateDefaultFile =
						alwaysCreateDefaultFile &&
						!exclusions.Any(x => pagePath.EndsWith(
							x, StringComparison.OrdinalIgnoreCase));

					pagePath +=
						generateDefaultFile
						? $"{Path.DirectorySeparatorChar}{indexFileName}"
						: pageFileExtension;
				}
			}

			pagePath = pagePath.ToFileSysPath().EnsureNotStartsWith(Path.DirectorySeparatorChar);

			return Path.Combine(rootFolder.ToFileSysPath(), pagePath);
		}

		public static string ToFileSysPath(this string path) =>
			path is null ? string.Empty : path
			.Replace(Consts.BakSlash, Path.DirectorySeparatorChar)
			.Replace(Consts.FwdSlash, Path.DirectorySeparatorChar)
			;

		public static string StripQueryString(this string url)
		{
			//Throw.IfNullOrWhiteSpace(
			//	url, nameof(url),
			//	Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace);

			if (string.IsNullOrWhiteSpace(url)) return url;

			var idx = url.IndexOf('?');
			return idx > -1 ? url[0..idx] : url;
		}


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
						!string.IsNullOrWhiteSpace(page.OutFile)
						? page.OutFile.Replace(Consts.BakSlash, Consts.FwdSlash).EnsureStartsWith(Consts.FSlash)
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
