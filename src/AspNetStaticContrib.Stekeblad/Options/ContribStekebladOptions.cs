namespace AspNetStaticContrib.Stekeblad.Options
{
	/// <summary>
	/// Declare witch resource locators you intend to use
	/// so any required services or features can be registered.
	/// Nothing is enabled by default.
	/// </summary>
	public class ContribStekebladOptions
	{
		public bool RegisterActionDescriptorResourceLocator { get; set; }

		public bool RegisterSitemapResourceLocator { get; set; }
	}
}