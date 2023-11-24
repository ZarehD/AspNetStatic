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
	internal class StaticPageGeneratorConfig
	{
		public List<PageInfo> Pages { get; } = new();

		public string DestinationRoot { get; }

		public bool AlwaysCreateDefaultFile { get; }

		public bool UpdateLinks { get; }

		public string DefaultFileName { get; } = "index";

		public string PageFileExtension { get; } = ".html";

		public string IndexFileName => $"{this.DefaultFileName}{this.PageFileExtension.EnsureStartsWith('.')}";

		public List<string> DefaultFileExclusions { get; } = new(new[] { "index", "default" });

		public bool OptimizePageContent { get; }

		public IOptimizerSelector? OptimizerSelector { get; init; } = null!;


		public StaticPageGeneratorConfig(
			IEnumerable<PageInfo> pages,
			string destinationRoot,
			bool createDefaultFile,
			bool fixupHrefValues,
			bool enableOptimization,
			IOptimizerSelector? optimizerSelector = default)
		{
			this.DestinationRoot = Throw.IfNullOrWhitespace(destinationRoot);
			this.AlwaysCreateDefaultFile = createDefaultFile;
			this.UpdateLinks = fixupHrefValues;

			if ((pages is not null) && pages.Any())
			{
				this.Pages.AddRange(pages);
			}

			this.OptimizePageContent = enableOptimization;
			this.OptimizerSelector = Throw.IfNull(optimizerSelector, () => this.OptimizePageContent);
		}

		public StaticPageGeneratorConfig(
			IEnumerable<PageInfo> pages,
			string destinationRoot,
			bool createDefaultFile,
			bool fixupHrefValues,
			string defaultFileName,
			string fileExtension,
			IEnumerable<string> defaultFileExclusions,
			bool enableOptimization = default,
			IOptimizerSelector? optimizerSelector = default)
			: this(pages, destinationRoot, createDefaultFile, fixupHrefValues,
				  enableOptimization, optimizerSelector)
		{
			this.DefaultFileName = Throw.IfNullOrWhitespace(defaultFileName);
			this.PageFileExtension = Throw.IfNullOrWhitespace(fileExtension);

			if ((defaultFileExclusions is not null) && defaultFileExclusions.Any())
			{
				this.DefaultFileExclusions.Clear();
				this.DefaultFileExclusions.AddRange(defaultFileExclusions);
			}
		}
	}
}
