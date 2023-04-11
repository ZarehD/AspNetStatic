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
	public static class PageInfoExtensions
	{
		public static bool ContainsPageForRoute(
			this IEnumerable<PageInfo> pages, string route,
			bool routesAreCaseSensitive = default) =>
			(pages is not null) && pages.Any() &&
			(pages.GetForRoute(route, routesAreCaseSensitive) is not null);


		public static PageInfo? GetForRoute(
			this IEnumerable<PageInfo> pages, string route,
			bool routesAreCaseSensitive = default) =>
			(pages is null) || !pages.Any() ? default :
			pages.FirstOrDefault(
				p => p.Route.EnsureStartsWith(Consts.FwdSlash).EnsureEndsWith(Consts.FwdSlash)
				.Equals(route.EnsureStartsWith(Consts.FwdSlash).EnsureEndsWith(Consts.FwdSlash),
					routesAreCaseSensitive
					? StringComparison.InvariantCulture
					: StringComparison.InvariantCultureIgnoreCase));
	}
}
