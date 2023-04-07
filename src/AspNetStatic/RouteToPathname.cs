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
	internal class RouteToPathname
	{
		public static string GetPathname(
			PageInfo page, 
			string rootFolder,
			bool alwaysCreateDefaultFile,
			string indexFileName,
			string pageFileExtension,
			string[] exclusions)
		{
			if (page is null)
			{
				throw new ArgumentNullException(nameof(page));
			}
			if (string.IsNullOrWhiteSpace(rootFolder))
			{
				throw new ArgumentException(
					Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace,
					nameof(rootFolder));
			}

			var pagePath = string.Empty;

			if (!string.IsNullOrWhiteSpace(page.OutFilePathname))
			{
				pagePath = page.OutFilePathname;
			}
			else
			{
				if (string.IsNullOrWhiteSpace(indexFileName))
				{
					throw new ArgumentException(
						Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace,
						nameof(indexFileName));
				}
				if (string.IsNullOrWhiteSpace(pageFileExtension))
				{
					throw new ArgumentException(
						Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace,
						nameof(pageFileExtension));
				}

				exclusions ??= Array.Empty<string>();

				var pageUrl = GetUrlWithoutQueryString(page.Route);
				var uriKind = UriKind.Relative;

				pagePath =
					Uri.IsWellFormedUriString(pageUrl, uriKind)
					? pageUrl : throw new InvalidOperationException(string.Format(
						CultureInfo.InvariantCulture,
						Properties.Resources.Err_RouteForPageNotWellFormed,
						uriKind.ToString()));

				if (pagePath.EndsWith(RouteConsts.FwdSlash) || pagePath.EndsWith(RouteConsts.BakSlash))
				{
					pagePath += indexFileName;
				}
				else if (string.IsNullOrEmpty(Path.GetExtension(pagePath)))
				{
					var generateDefaultFile =
						alwaysCreateDefaultFile &&
						!exclusions.Any(x => pagePath.EndsWith(
							x, StringComparison.InvariantCultureIgnoreCase));

					pagePath +=
						generateDefaultFile
						? $"{RouteConsts.FwdSlash}{indexFileName}"
						: pageFileExtension;
				}
			}

			pagePath = pagePath
				.Replace(RouteConsts.BakSlash, Path.DirectorySeparatorChar)
				.Replace(RouteConsts.FwdSlash, Path.DirectorySeparatorChar)
				;

			return
				Path.Combine(rootFolder,
				pagePath.AssureNotStartsWith(Path.DirectorySeparatorChar));
		}


		public static string GetUrlWithoutQueryString(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
			{
				throw new ArgumentException(
					Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace,
					nameof(url));
			}

			var idx = url.IndexOf('?');
			return idx > -1 ? url[0..idx] : url;
		}
	}
}
