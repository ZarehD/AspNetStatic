/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023-2025 Zareh DerGevorkian

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License. 
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
--------------------------------------------------------------------------------------------------------------------------------*/

/* IMPORTANT NOTE:
 * 
 * The methods implemented in this class are not intended to be 
 * comprehensive or even broadly applicable. Their purpose is 
 * to provide a starting point for more complex, meaningful, 
 * and/or broadly applicable implementations contributed by 
 * the AspNetStatic community.
 * 
 */

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileSystemGlobbing;
using FSGA = Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace AspNetStaticContrib.AspNetStatic;

public static class StaticResourcesInfoProviderExtensions
{
	/// <summary>
	///		Adds all viewable razor pages in the project to 
	///		the <see cref="StaticResourcesInfoProvider"/> instance.
	/// </summary>
	/// <remarks>
	///		<para>
	///			Expectes Razor Page files to be located in the Pages 
	///			subfolder in the project root folder.
	///		</para>
	///		<para>
	///			Pages considered 'viewable are those that are not 
	///			located in a subfolder named 'Shared', or whose 
	///			filename does not start with the '_' character.
	///		</para>
	/// </remarks>
	/// <param name="provider">The <see cref="StaticResourcesInfoProvider"/> instance.</param>
	/// <param name="env">An instance of <see cref="IWebHostEnvironment"/>.</param>
	/// <returns>The object referenced in <paramref name="provider"/>.</returns>
	public static StaticResourcesInfoProvider AddAllProjectRazorPages(
		this StaticResourcesInfoProvider provider,
		IWebHostEnvironment env)
	{
		Throw.IfNull(provider);
		Throw.IfNull(env);

		const string razorExtension = ".cshtml";
		const StringComparison compMode = StringComparison.Ordinal;

		var sharedFolder = string.Format("{0}Shared{0}", Path.DirectorySeparatorChar);
		var pagesFolderPath = Path.Combine(env.ContentRootPath, "Pages");
		var razorPageFiles = Directory.GetFiles(pagesFolderPath, $"*{razorExtension}", SearchOption.AllDirectories);
		var razorPageRoutes = razorPageFiles
			.Where(f =>
				!f.Contains($"{Path.DirectorySeparatorChar}_", compMode) &&
				!f.Contains(sharedFolder, compMode))
			.Select(f => f
				.Replace(pagesFolderPath, string.Empty)
				.Replace(razorExtension, string.Empty)
				.Replace(Path.DirectorySeparatorChar, '/'));

		provider.Add(razorPageRoutes.Select(route => new PageResource(route)));

		return provider;
	}


	/// <summary>
	///		Adds all CSS, JS and binary content found in the project wwwroot folder 
	///		to the <see cref="StaticResourcesInfoProvider"/> instance.
	/// </summary>
	/// <remarks>
	///		Any file that is not considered to be a CSS or JS file is deemed to be 
	///		a binary resource, and therefore added as a <see cref="BinResource"/>.
	/// </remarks>
	/// <param name="provider">The <see cref="StaticResourcesInfoProvider"/> instance.</param>
	/// <param name="env">An instance of <see cref="IWebHostEnvironment"/>.</param>
	/// <returns>The object referenced in <paramref name="provider"/>.</returns>
	public static StaticResourcesInfoProvider AddAllWebRootContent(
		this StaticResourcesInfoProvider provider,
		IWebHostEnvironment env)
	{
		Throw.IfNull(provider);
		Throw.IfNull(env);

		var webRootPath = env.WebRootPath;
		var allWebRootFiles = Directory
			.GetFiles(webRootPath, "*.*", SearchOption.AllDirectories)
			.Select(f => f
				.Replace(webRootPath, string.Empty)
				.Replace(Path.DirectorySeparatorChar, '/'))
			;

#if NET8_0_OR_GREATER
		string[] cssExts = [".css", ".scss"];
		string[] jsExts = [".js", ".json"];
#else
		string[] cssExts = new[] { ".css", ".scss" };
		string[] jsExts = new[] { ".js", ".json" };
#endif
		var comparer = StringComparer.OrdinalIgnoreCase;

		provider.Add(allWebRootFiles
			.Where(r => cssExts.Contains(Path.GetExtension(r), comparer))
			.Select(r => new CssResource(r)))
			;

		provider.Add(allWebRootFiles
			.Where(r => jsExts.Contains(Path.GetExtension(r), comparer))
			.Select(r => new JsResource(r)))
			;

		provider.Add(allWebRootFiles
			.Where(r =>
				!cssExts.Contains(Path.GetExtension(r), comparer) &&
				!jsExts.Contains(Path.GetExtension(r), comparer)
				)
			.Select(r => new BinResource(r)))
			;

		return provider;
	}


	/// <summary>
	///		Retrieves a collection of <see cref="CssResource"/> for files 
	///		found in the wwwroot folder based on the specified filters.
	/// </summary>
	/// <param name="hostEnvironment">The <see cref="IWebHostEnvironment"/> instance.</param>
	/// <param name="include">
	///		The glob filter for files to include.
	///		Defaults to "**/*.css", if empty.
	/// </param>
	/// <param name="exclude">The glob filter for files to exclude.</param>
	/// <returns>A collection of zero or more <see cref="CssResource"/> objects.</returns>
	public static IEnumerable<CssResource> GetWebRootCssResources(
		this IWebHostEnvironment hostEnvironment,
		string[]? include = default,
		string[]? exclude = default)
	{
		Throw.IfNull(hostEnvironment);

		var webRootPath = hostEnvironment.WebRootPath;

		var routes = GetResourceRoutes(
			webRootPath,
			include ?? ["**/*.css"],
			exclude ?? []);

		return
			(0 < routes.Length)
			? routes.Select(x => new CssResource(x))
			: []
			;
	}

	/// <summary>
	///		Retrieves a collection of <see cref="JsResource"/> for files 
	///		found in the wwwroot folder based on the specified filters.
	/// </summary>
	/// <param name="hostEnvironment">The <see cref="IWebHostEnvironment"/> instance.</param>
	/// <param name="include">
	///		The glob filter for files to include.
	///		Defaults to "**/*.js", if empty.
	/// </param>
	/// <param name="exclude">The glob filter for files to exclude.</param>
	/// <returns>A collection of zero or more <see cref="JsResource"/> objects.</returns>
	public static IEnumerable<JsResource> GetWebRootJsResources(
		this IWebHostEnvironment hostEnvironment,
		string[]? include = default,
		string[]? exclude = default)
	{
		Throw.IfNull(hostEnvironment);

		var webRootPath = hostEnvironment.WebRootPath;

		var routes = GetResourceRoutes(
			webRootPath,
			include ?? ["**/*.js"],
			exclude ?? []);

		return
			(0 < routes.Length)
			? routes.Select(x => new JsResource(x))
			: []
			;
	}

	/// <summary>
	///		Retrieves a collection of <see cref="BinResource"/> for files 
	///		found in the wwwroot folder based on the specified filters.
	/// </summary>
	/// <param name="hostEnvironment">The <see cref="IWebHostEnvironment"/> instance.</param>
	/// <param name="include">The glob filter for files to include.</param>
	/// <param name="exclude">The glob filter for files to exclude.</param>
	/// <returns>A collection of zero or more <see cref="BinResource"/> objects.</returns>
	public static IEnumerable<BinResource> GetWebRootBinResources(
		this IWebHostEnvironment hostEnvironment,
		string[] include,
		string[]? exclude = default)
	{
		Throw.IfNull(hostEnvironment);
		Throw.InvalidOpWhen(
			() => 0 == include.Length,
			"Inclusion filter is empty when adding binary static resources.");

		var webRootPath = hostEnvironment.WebRootPath;

		var routes = GetResourceRoutes(
			webRootPath,
			include!,
			exclude ?? []);

		return
			(0 < routes.Length)
			? routes.Select(x => new BinResource(x))
			: []
			;
	}

	/// <summary>
	///		Retrieves a collection of routes for resources found in 
	///		the specified root folder using the provided filters.
	/// </summary>
	/// <param name="rootPath">The root folder where to start the search.</param>
	/// <param name="includeFilter">The glob filter for files to include.</param>
	/// <param name="excludeFilter">The glob filter for files to exclude.</param>
	/// <returns>A collection containing zero or more resource route entries.</returns>
	public static string[] GetResourceRoutes(
		string rootPath,
		string[] includeFilter,
		string[] excludeFilter)
	{
		Throw.IfNullOrWhitespace(rootPath);

		var matcher = new Matcher();
		matcher.AddIncludePatterns(includeFilter);
		matcher.AddExcludePatterns(excludeFilter);
		var result = matcher.Execute(
			new FSGA.DirectoryInfoWrapper(
				new DirectoryInfo(rootPath)));

		return
			result.HasMatches
			? result.Files
				.Select(f => f.Path.Replace(Path.DirectorySeparatorChar, '/')) // as route
				.Select(f => f.Replace(rootPath, string.Empty))                // relative to root path
				.ToArray()
			: []
			;
	}
}
