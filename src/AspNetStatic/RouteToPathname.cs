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
	internal static class RouteToPathname
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

		public static string StripQueryString(this string url)
		{
			//Throw.IfNullOrWhiteSpace(
			//	url, nameof(url),
			//	Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace);

			if (string.IsNullOrWhiteSpace(url)) return url;

			var idx = url.IndexOf('?');
			return idx > -1 ? url[0..idx] : url;
		}
	}
}
