/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023-2025 Zareh DerGevorkian

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License. 
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
--------------------------------------------------------------------------------------------------------------------------------*/

namespace AspNetStatic
{
	public static class ResourceInfoExtensions
	{
		public static bool ContainsResourceForRoute(
			this IEnumerable<ResourceInfoBase> resources, string route,
			bool routesAreCaseSensitive = default) =>
			(resources is not null) && resources.Any() &&
			(resources.GetResourceForRoute(route, routesAreCaseSensitive) is not null);

		public static bool ContainsResourceForUrl(
			this IEnumerable<ResourceInfoBase> resources, string url,
			bool routesAreCaseSensitive = default) =>
			(resources is not null) && resources.Any() &&
			(resources.GetResourceForUrl(url, routesAreCaseSensitive) is not null);


		public static ResourceInfoBase? GetResourceForRoute(
			this IEnumerable<ResourceInfoBase> resources, string route,
			bool routesAreCaseSensitive = default) =>
			(resources is null) || !resources.Any() ? default :
			resources.FirstOrDefault(
				p => route.Equals(Consts.FSlash)
				? p.Route.Equals(Consts.FSlash)
				: p.Route.NormalizeRoute().Equals(
					route.NormalizeRoute(),
					routesAreCaseSensitive
					? StringComparison.Ordinal
					: StringComparison.OrdinalIgnoreCase));

		public static ResourceInfoBase? GetResourceForUrl(
			this IEnumerable<ResourceInfoBase> resources, string url,
			bool routesAreCaseSensitive = default) =>
			(resources is null) || !resources.Any() ? default :
			resources.FirstOrDefault(
				p => url.Equals(Consts.FSlash)
				? p.Url.Equals(Consts.FSlash)
				: p.Url.NormalizeUrl().Equals(
					url.NormalizeUrl(),
					routesAreCaseSensitive
					? StringComparison.Ordinal
					: StringComparison.OrdinalIgnoreCase));


		public static PageResource? GetPageResourceForStaticPage(
			this IEnumerable<PageResource> resources,
			PageResourceFinderConfig config,
			string staticPageRoute) =>
			(resources is null) || !resources.Any() ? default :
			resources.FirstOrDefault(
				p =>
				staticPageRoute.ToFileSysPath().HasSameText(
					string.IsNullOrWhiteSpace(p.OutFile)
					? p.GetOutFilePathname(
						Consts.BSlash,
						alwaysCreateDefaultFile: config.AlwaysDefaultFile,
						indexFileName: config.IndexFileName,
						pageFileExtension: config.PageFileExtension,
						exclusions: config.DefaultFileExclusions)
					: p.OutFile.ToFileSysPath().EnsureStartsWith(Consts.BakSlash)
				));

		public record PageResourceFinderConfig(
			bool AlwaysDefaultFile,
			string IndexFileName,
			string PageFileExtension,
			string[] DefaultFileExclusions
		);



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
