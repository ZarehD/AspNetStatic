/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023-2024 Zareh DerGevorkian

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License. 
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
--------------------------------------------------------------------------------------------------------------------------------*/

using Microsoft.AspNetCore.Hosting;

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
			.Where(p =>
				!p.Contains($"{Path.DirectorySeparatorChar}_", compMode) &&
				!p.Contains(sharedFolder, compMode))
			.Select(p => p
				.Replace(pagesFolderPath, string.Empty)
				.Replace(razorExtension, string.Empty)
				.Replace(Path.DirectorySeparatorChar, '/'));

		provider.Add(razorPageRoutes.Select(p => new PageResource(p)));

		return provider;
	}
}
