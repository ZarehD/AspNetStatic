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
	public static class Helpers
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


		public static string ToFileSysPath(this string path) =>
			path is null ? string.Empty : path
			.Replace(Consts.BakSlash, Path.DirectorySeparatorChar)
			.Replace(Consts.FwdSlash, Path.DirectorySeparatorChar)
			;
	}
}
