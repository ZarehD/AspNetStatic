/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023 Zareh DerGevorkian

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
			Throw.IfNull(page, nameof(page));
			Throw.IfNullOrWhiteSpace(
				rootFolder, nameof(rootFolder),
				Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace);

			var pagePath = string.Empty;

			if (!string.IsNullOrWhiteSpace(page.OutFilePathname))
			{
				pagePath = page.OutFilePathname;
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

				var pageUrl = GetUrlWithoutQueryString(page.Route);
				var uriKind = UriKind.Relative;

				if (!Uri.IsWellFormedUriString(pageUrl, uriKind))
				{
					Throw.InvalidOp(Properties.Resources.Err_RouteForPageNotWellFormed, uriKind.ToString());
				}
				pagePath = pageUrl;

				if (pagePath.EndsWith(Consts.FwdSlash) || pagePath.EndsWith(Consts.BakSlash))
				{
					pagePath += indexFileName;
				}
				else if (string.IsNullOrEmpty(Path.GetExtension(pagePath)))
				{
					var generateDefaultFile =
						alwaysCreateDefaultFile &&
						!exclusions.Any(x => pagePath.EndsWith(
							x, StringComparison.OrdinalIgnoreCase));

					pagePath +=
						generateDefaultFile
						? $"{Consts.FwdSlash}{indexFileName}"
						: pageFileExtension;
				}
			}

			pagePath = pagePath
				.Replace(Consts.BakSlash, Path.DirectorySeparatorChar)
				.Replace(Consts.FwdSlash, Path.DirectorySeparatorChar)
				;

			return
				Path.Combine(rootFolder,
				pagePath.EnsureNotStartsWith(Path.DirectorySeparatorChar));
		}


		public static string GetUrlWithoutQueryString(string url)
		{
			Throw.IfNullOrWhiteSpace(
				url, nameof(url),
				Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace);

			var idx = url.IndexOf('?');
			return idx > -1 ? url[0..idx] : url;
		}
	}
}
