/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023-2025 Zareh DerGevorkian

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License. 
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
--------------------------------------------------------------------------------------------------------------------------------*/

using AspNetStatic.Optimizer;

namespace AspNetStatic;

internal sealed class StaticGeneratorConfig
{
	public List<ResourceInfoBase> Resources { get; } = [];

	public IEnumerable<PageResource> Pages =>
		this.Resources
		.Where(r => r?.GetType() == Consts.TypeOfPageResource)
		.Cast<PageResource>();

	public IEnumerable<NonPageResource> OtherResources =>
		this.Resources
		.Where(r => r?.GetType() != Consts.TypeOfPageResource)
		.Cast<NonPageResource>();

	public bool SkipPageResources { get; init; }

	public bool SkipCssResources { get; init; }

	public bool SkipJsResources { get; init; }

	public bool SkipBinResources { get; init; }

	public string DestinationRoot { get; }

	public bool AlwaysCreateDefaultFile { get; init; }

	public bool UpdateLinks { get; init; }

	public string DefaultFileName { get; } = Consts.DefaultIndexFile;

	public string PageFileExtension { get; } = Consts.Ext_Htm;

	public string IndexFileName => $"{this.DefaultFileName}{this.PageFileExtension.EnsureStartsWith('.')}";

	public List<string> DefaultFileExclusions { get; } = new(Consts.DefaultFileExclusions);

	public bool OptimizePageContent { get; }

	public IOptimizerSelector? OptimizerSelector { get; init; } //= null!;


	public StaticGeneratorConfig(
		IEnumerable<ResourceInfoBase>? resources,
		string destinationRoot,
		bool createDefaultFile,
		bool fixupHrefValues,
		bool enableOptimization,
		IOptimizerSelector? optimizerSelector = default,
		bool skipPageResources = default,
		bool skipCssResources = default,
		bool skipJsResources = default,
		bool skipBinResources = default)
	{
		this.DestinationRoot = Throw.IfNullOrWhitespace(destinationRoot);
		this.AlwaysCreateDefaultFile = createDefaultFile;
		this.UpdateLinks = fixupHrefValues;
		this.OptimizePageContent = enableOptimization;
		this.OptimizerSelector = Throw.IfNull(optimizerSelector, () => this.OptimizePageContent);
		this.SkipPageResources = skipPageResources;
		this.SkipCssResources = skipCssResources;
		this.SkipJsResources = skipJsResources;
		this.SkipBinResources = skipBinResources;

		if ((resources is not null) && resources.Any())
		{
			this.Resources.AddRange(resources.Where(r => r is not null));
		}
	}

	public StaticGeneratorConfig(
		IEnumerable<ResourceInfoBase>? resources,
		string destinationRoot,
		bool createDefaultFile,
		bool fixupHrefValues,
		string defaultFileName,
		string fileExtension,
		IEnumerable<string>? defaultFileExclusions,
		bool enableOptimization = default,
		IOptimizerSelector? optimizerSelector = default,
		bool skipPageResources = default,
		bool skipCssResources = default,
		bool skipJsResources = default,
		bool skipBinResources = default)
		: this(resources, destinationRoot, createDefaultFile, fixupHrefValues,
			  enableOptimization, optimizerSelector,
			  skipPageResources, skipCssResources,
			  skipJsResources, skipBinResources)
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
