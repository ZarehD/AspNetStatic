/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023 Zareh DerGevorkian

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License. 
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
--------------------------------------------------------------------------------------------------------------------------------*/

using WebMarkupMin.Core;

namespace AspNetStatic
{
	public class StaticPageGeneratorConfig
	{
		public List<PageInfo> Pages { get; } = new();

		public string DestinationRoot { get; }

		public bool AlwaysCreateDefaultFile { get; }

		public bool UpdateLinks { get; }

		public string DefaultFileName { get; } = "index";

		public string PageFileExtension { get; } = ".html";

		public string IndexFileName => $"{this.DefaultFileName}{this.PageFileExtension.EnsureStartsWith('.')}";

		public List<string> DefaultFileExclusions { get; } = new(new[] { "index", "default" });

		public bool OptimizePageContent { get; init; } = true;

		public ICssMinifier? CssMinifier { get; init; }

		public IJsMinifier? JsMinifier { get; init; }

		public HtmlMinificationSettings? HtmlMinifierSettings { get; init; }


		public StaticPageGeneratorConfig(
			IEnumerable<PageInfo> pages,
			string destinationRoot,
			bool createDefaultFile,
			bool fixupHrefValues)
		{
			this.DestinationRoot = Throw.IfNullOrWhiteSpace(destinationRoot, nameof(destinationRoot), Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace);
			this.AlwaysCreateDefaultFile = createDefaultFile;
			this.UpdateLinks = fixupHrefValues;

			if ((pages is not null) && pages.Any())
			{
				this.Pages.AddRange(pages);
			}
		}

		public StaticPageGeneratorConfig(
			IEnumerable<PageInfo> pages,
			string destinationRoot,
			bool createDefaultFile,
			bool fixupHrefValues,
			bool disableOptimizations,
			HtmlMinificationSettings? htmlMinifierSettings = default,
			ICssMinifier? cssMinifier = default,
			IJsMinifier? jsMinifier = default)
			: this(pages, destinationRoot, createDefaultFile, fixupHrefValues)
		{
			this.OptimizePageContent = !disableOptimizations;
			this.HtmlMinifierSettings = htmlMinifierSettings;
			this.CssMinifier = cssMinifier;
			this.JsMinifier = jsMinifier;
		}

		public StaticPageGeneratorConfig(
			IEnumerable<PageInfo> pages,
			string destinationRoot,
			bool createDefaultFile,
			bool fixupHrefValues,
			string defaultFileName,
			string fileExtension,
			IEnumerable<string> defaultFileExclusions,
			bool disableOptimizations = default,
			HtmlMinificationSettings? htmlMinifierSettings = default,
			ICssMinifier? cssMinifier = default,
			IJsMinifier? jsMinifier = default)
			: this(pages, destinationRoot, createDefaultFile, fixupHrefValues,
				  disableOptimizations, htmlMinifierSettings, cssMinifier, jsMinifier)
		{
			this.DefaultFileName = Throw.IfNullOrWhiteSpace(defaultFileName, nameof(defaultFileName), Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace);
			this.PageFileExtension = Throw.IfNullOrWhiteSpace(fileExtension, nameof(fileExtension), Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace);

			if ((defaultFileExclusions is not null) && defaultFileExclusions.Any())
			{
				this.DefaultFileExclusions.Clear();
				this.DefaultFileExclusions.AddRange(defaultFileExclusions);
			}
		}
	}
}
