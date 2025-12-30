namespace AspNetStaticContrib.Stekeblad.Options
{
	/// <summary>
	/// Forwarded information from the
	/// <see cref="LocatingStaticResourcesInfoProvider"/>
	/// to a <see cref="ResourceLocatorBase">ResourceLocator</see>
	/// to make it aware of various settings.
	/// </summary>
	public record ResourceLocatorFilter
	{
		public bool SkipPageResources { get; init; }

		public bool SkipCssResources { get; init; }

		public bool SkipJsResources { get; init; }

		public bool SkipBinResources { get; init; }
	}
}
