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
	public static class Helpers
	{
		public static string ToDefaultFileFallback(
			this string route, string[] exclusions,
			string defaultFileName, string pageFileExtension)
		{
			if (string.IsNullOrWhiteSpace(route))
			{
				throw new ArgumentException(
					Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace,
					nameof(route));
			}
			if (string.IsNullOrWhiteSpace(defaultFileName))
			{
				throw new ArgumentException(
					Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace,
					nameof(defaultFileName));
			}
			if (string.IsNullOrWhiteSpace(pageFileExtension))
			{
				throw new ArgumentException(
					Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace,
					nameof(pageFileExtension));
			}

			return
				((exclusions is null) || !exclusions.Any(
					x => route.EnsureNotEndsWith(RouteConsts.FwdSlash)?
					.EndsWith(x, StringComparison.InvariantCultureIgnoreCase) ?? false))
				? route.EnsureEndsWith(RouteConsts.FwdSlash) + defaultFileName
				: route.EnsureNotEndsWith(RouteConsts.FwdSlash) + pageFileExtension
				;
		}
	}
}
