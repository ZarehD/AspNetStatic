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
	public interface IStaticResourcesInfoProvider
	{
		IEnumerable<ResourceInfoBase> Resources { get; }

		IEnumerable<PageResource> PageResources { get; }

		IEnumerable<NonPageResource> OtherResources { get; }

		/// <summary>
		///		<para>
		///			Gets the name (without an extension) to use for the default file 
		///			created routes that end wuth a slash.
		///		</para>
		///		<para>
		///			Example: "index"
		///		</para>
		/// </summary>
		string DefaultFileName { get; }

		/// <summary>
		///		<para>
		///			Gets the file extension to use for generated static page files.
		///		</para>
		///		<para>
		///			Example: ".html"
		///		</para>
		/// </summary>
		string PageFileExtension { get; }

		/// <summary>
		///		<para>
		///			Gets a collection of page/file names (without file extension) 
		///			for which a default file will not be creaed (an html file will 
		///			be created instead).
		///		</para>
		///		<para>
		///			If "index" is in the list, a route for a page /index will create 
		///			the file /index.html instead of /index/index.html. However, for 
		///			a route /index/ the result will be /index/index.html.
		///		</para>
		/// </summary>
		string[] DefaultFileExclusions { get; }

		/// <summary>
		///		Gets a value that indicates whether to skip generating static files 
		///		for entries in the <see cref="PageResources"/> collection.
		/// </summary>
		public bool SkipPageResources { get; }

		/// <summary>
		///		Gets a value that indicates whether to skip generating static files for 
		///		<see cref="CssResource"/> entries in the <see cref="OtherResources"/> collection.
		/// </summary>
		public bool SkipCssResources { get; }

		/// <summary>
		///		Gets a value that indicates whether to skip generating static files for 
		///		<see cref="JsResource"/> entries in the <see cref="OtherResources"/> collection.
		/// </summary>
		public bool SkipJsResources { get; }

		/// <summary>
		///		Gets a value that indicates whether to skip generating static files for 
		///		<see cref="BinResource"/> entries in the <see cref="OtherResources"/> collection.
		/// </summary>
		public bool SkipBinResources { get; }
	}
}
