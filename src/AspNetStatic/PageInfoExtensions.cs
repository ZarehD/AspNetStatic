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
			(pages.GetPageForRoute(route, routesAreCaseSensitive) is not null);

		public static bool ContainsPageForUrl(
			this IEnumerable<PageInfo> pages, string url,
			bool routesAreCaseSensitive = default) =>
			(pages is not null) && pages.Any() &&
			(pages.GetPageForUrl(url, routesAreCaseSensitive) is not null);


		public static PageInfo? GetPageForRoute(
			this IEnumerable<PageInfo> pages, string route,
			bool routesAreCaseSensitive = default) =>
			(pages is null) || !pages.Any() ? default :
			pages.FirstOrDefault(
				p => route.Equals(Consts.FSlash)
				? p.Route.Equals(Consts.FSlash)
				: p.Route.NormalizeRoute().Equals(
					route.NormalizeRoute(),
					routesAreCaseSensitive
					? StringComparison.Ordinal
					: StringComparison.OrdinalIgnoreCase));

		public static PageInfo? GetPageForUrl(
			this IEnumerable<PageInfo> pages, string url,
			bool routesAreCaseSensitive = default) =>
			(pages is null) || !pages.Any() ? default :
			pages.FirstOrDefault(
				p => url.Equals(Consts.FSlash)
				? p.Url.Equals(Consts.FSlash)
				: p.Url.NormalizeUrl().Equals(
					url.NormalizeUrl(),
					routesAreCaseSensitive
					? StringComparison.Ordinal
					: StringComparison.OrdinalIgnoreCase));


		private static string NormalizeRoute(this string? route) =>
			string.IsNullOrEmpty(route) ? string.Empty :
			route.EnsureStartsWith(Consts.FSlash).EnsureEndsWith(Consts.FSlash);

		private static string NormalizeUrl(this string? url) =>
			string.IsNullOrEmpty(url) ? string.Empty :
			url.EnsureStartsWith(Consts.FSlash)
			.EnsureNotEndsWith(Consts.FSlash)
			.EnsureNotEndsWith("?")
			;
	}
}
