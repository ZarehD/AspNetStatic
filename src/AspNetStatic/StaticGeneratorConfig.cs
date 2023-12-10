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
	internal class StaticGeneratorConfig
	{
		public List<PageResource> Pages { get; } = new();

		public List<NonPageResource> OtherResources { get; } = new();

		public bool SkipProcessingPageResources { get; }

		public bool SkipProcessingOtherResources { get; }

		public string DestinationRoot { get; }

		public bool AlwaysCreateDefaultFile { get; }

		public bool UpdateLinks { get; }

		public string DefaultFileName { get; } = Consts.DefaultIndexFile;

		public string PageFileExtension { get; } = Consts.Ext_Htm;

		public string IndexFileName => $"{this.DefaultFileName}{this.PageFileExtension.EnsureStartsWith('.')}";

		public List<string> DefaultFileExclusions { get; } = new(Consts.DefaultFileExclusions);

		public bool OptimizePageContent { get; }

		public IOptimizerSelector? OptimizerSelector { get; init; } //= null!;


		public StaticGeneratorConfig(
			IEnumerable<PageResource>? pages,
			string destinationRoot,
			bool createDefaultFile,
			bool fixupHrefValues,
			bool enableOptimization,
			IOptimizerSelector? optimizerSelector = default,
			bool skipProcessingPageResources = default)
		{
			this.DestinationRoot = Throw.IfNullOrWhitespace(destinationRoot);
			this.AlwaysCreateDefaultFile = createDefaultFile;
			this.UpdateLinks = fixupHrefValues;
			this.OptimizePageContent = enableOptimization;
			this.OptimizerSelector = Throw.IfNull(optimizerSelector, () => this.OptimizePageContent);
			this.SkipProcessingPageResources = skipProcessingPageResources;

			if ((pages is not null) && pages.Any())
			{
				this.Pages.AddRange(pages);
			}
		}

		public StaticGeneratorConfig(
			IEnumerable<PageResource>? pages,
			IEnumerable<NonPageResource>? otherResources,
			string destinationRoot,
			bool createDefaultFile,
			bool fixupHrefValues,
			bool enableOptimization,
			IOptimizerSelector? optimizerSelector = default,
			bool skipProcessingPageResources = default,
			bool skipProcessingOtherResources = default)
			: this(pages!, destinationRoot, createDefaultFile,
				  fixupHrefValues, enableOptimization, optimizerSelector,
				  skipProcessingPageResources)
		{
			this.SkipProcessingOtherResources = skipProcessingOtherResources;

			if ((otherResources is not null) && otherResources.Any())
			{
				this.OtherResources.AddRange(otherResources);
			}
		}

		public StaticGeneratorConfig(
			IEnumerable<PageResource>? pages,
			string destinationRoot,
			bool createDefaultFile,
			bool fixupHrefValues,
			string defaultFileName,
			string fileExtension,
			IEnumerable<string>? defaultFileExclusions,
			bool enableOptimization = default,
			IOptimizerSelector? optimizerSelector = default,
			bool skipProcessingPageResources = default)
			: this(pages, destinationRoot, createDefaultFile, fixupHrefValues,
				  enableOptimization, optimizerSelector,
				  skipProcessingPageResources)
		{
			this.DefaultFileName = Throw.IfNullOrWhitespace(defaultFileName);
			this.PageFileExtension = Throw.IfNullOrWhitespace(fileExtension);

			if ((defaultFileExclusions is not null) && defaultFileExclusions.Any())
			{
				this.DefaultFileExclusions.Clear();
				this.DefaultFileExclusions.AddRange(defaultFileExclusions);
			}
		}

		public StaticGeneratorConfig(
			IEnumerable<PageResource>? pages,
			IEnumerable<NonPageResource>? otherResources,
			string destinationRoot,
			bool createDefaultFile,
			bool fixupHrefValues,
			string defaultFileName,
			string fileExtension,
			IEnumerable<string>? defaultFileExclusions,
			bool enableOptimization = default,
			IOptimizerSelector? optimizerSelector = default,
			bool skipProcessingPageResources = default,
			bool skipProcessingOtherResources = default)
			: this(pages, destinationRoot, createDefaultFile, fixupHrefValues,
				  defaultFileName, fileExtension, defaultFileExclusions, 
				  enableOptimization, optimizerSelector,
				  skipProcessingPageResources)
		{
			this.SkipProcessingOtherResources = skipProcessingOtherResources;

			if ((otherResources is not null) && otherResources.Any())
			{
				this.OtherResources.AddRange(otherResources);
			}
		}
	}
}
