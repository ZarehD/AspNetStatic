using AspNetStaticContrib.Stekeblad.Options;

namespace AspNetStaticContrib.Stekeblad.ResourceLocators.Sitemap
{
	/// <summary>
	/// Options for controlling how <see cref="SitemapPageResourceLocator" /> behaves.
	/// </summary>
	/// <seealso cref="ResourceLocatorOptions"/>
	public class SitemapResourceLocatorOptions : ResourceLocatorOptions
	{
		/// <summary>
		/// List of paths to all sitemaps to process. Must not contain
		/// protocol or domain. Default value: [ "/sitemap.xml" ]
		/// </summary>
		public List<string> Sitemaps { get; set; } = ["/sitemap.xml"];
	}
}
