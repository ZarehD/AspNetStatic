/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023-2025 Zareh DerGevorkian

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License. 
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
--------------------------------------------------------------------------------------------------------------------------------*/

namespace AspNetStatic;

public abstract class StaticResourcesInfoProviderBase : IStaticResourcesInfoProvider
{
	protected readonly List<ResourceInfoBase> resources = [];

	public IEnumerable<ResourceInfoBase> Resources => [.. this.resources];

	public IEnumerable<PageResource> PageResources =>
		this.resources
		.Where(r => r?.GetType() == Consts.TypeOfPageResource)
		.Cast<PageResource>();

	public IEnumerable<NonPageResource> OtherResources =>
		this.resources
		.Where(r => r?.GetType() != Consts.TypeOfPageResource)
		.Cast<NonPageResource>();

	/// <inheritdoc/>
	/// <remarks>
	///		Defaults to "index".
	/// </remarks>
	public string DefaultFileName => this.defaultFileName;
	protected string defaultFileName = Consts.DefaultIndexFile;

	/// <inheritdoc/>
	/// <remarks>
	///		Defaults to ".html".
	/// </remarks>
	public string PageFileExtension => this.defaultFileExtension;
	protected string defaultFileExtension = Consts.Ext_Htm;

	/// <inheritdoc/>
	/// <remarks>
	///		Defaults to ["index", "default"].
	/// </remarks>
	public string[] DefaultFileExclusions => [.. this.exclusions];
	protected readonly List<string> exclusions = new(Consts.DefaultFileExclusions);

	public bool SkipPageResources { get; init; }

	public bool SkipCssResources { get; init; }

	public bool SkipJsResources { get; init; }

	public bool SkipBinResources { get; init; }


	protected virtual void SetDefaultFileName(string name) => this.defaultFileName = Throw.IfNullOrWhitespace(name, SR.Err_MissingDefaultFileName);
	protected virtual void SetDefaultFileExtension(string extension) => this.defaultFileExtension = Throw.IfNullOrWhitespace(extension, SR.Err_MissingDefaultFileExtension);
	protected virtual void SetDefaultFileExclusions(string[] newExclusions)
	{
		Throw.BadArgWhen(
			() => newExclusions.Any(s => string.IsNullOrWhiteSpace(s)),
			SR.Err_ArrayElementNullOrWhitespace,
			nameof(newExclusions));

		this.exclusions.Clear();
		this.exclusions.AddRange(newExclusions);
	}
}
