using AspNetStatic;
using AspNetStaticContrib.Stekeblad.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace AspNetStaticContrib.Stekeblad.ResourceLocators.Sitemap
{
	public static class SitemapLocationExtensions
	{
		/// <summary>
		/// <para>
		/// Creates an instance of
		/// <see cref="SitemapPageResourceLocator" />
		/// with the given <paramref name="locatorOptions"/>
		/// and adds it to the <paramref name="resourceProvider"/>
		/// </para>
		/// <para>
		/// If you call this method then you need to configure this feature with
		/// <see cref="Extensions.ServiceCollectionExtensions.AddAspNetStaticContribStekeblad"/>
		/// and call <see cref="Extensions.IHostExtensions.LocateStaticResources" /> before
		/// <see cref="StaticGeneratorHostExtension.GenerateStaticContent"/>
		/// </para>
		/// </summary>
		/// <param name="resourceProvider">Resource provider to add the resource locator to</param>
		/// <param name="locatorOptions">
		/// Settings to control how the resource locator creates different
		/// <see cref="ResourceInfoBase">resource info</see> objects as well as
		/// defining your site's sitemap paths.
		/// </param>
		/// <returns>The resource provider, to enable chaining of add calls</returns>
		/// <exception cref="InvalidOperationException">If resources already have been located.
		/// Resources can not be added or removed after generation has started or between updates
		/// (if update interval is configured)</exception>
		public static LocatingStaticResourcesInfoProvider AddSitemapLocator(this LocatingStaticResourcesInfoProvider resourceProvider,
			SitemapResourceLocatorOptions? locatorOptions = null)
		{
			var locator = new SitemapPageResourceLocator(locatorOptions ?? new());
			resourceProvider.AddResourceLocator(locator);
			return resourceProvider;
		}
	}

	/// <summary>
	/// <para>
	/// This resource locator discovers pages on the website by retrieving
	/// it's sitemap and creates <see cref="PageResource"/>s for each item.
	/// By default it only looks at the path /sitemap.xml but this can be
	/// changed by defining one or more paths in the
	/// <see cref="SitemapResourceLocatorOptions" /> instance passed to
	/// the constructor.
	/// </para>
	/// <para>
	/// This resource locator returns immediately if
	/// <see cref="StaticResourcesInfoProviderBase.SkipPageResources" />
	/// on the provider is set to true.
	/// </para>
	/// </summary>
	public class SitemapPageResourceLocator : ResourceLocatorBase
	{
		private readonly List<string> _sitemaps;
		public SitemapPageResourceLocator(SitemapResourceLocatorOptions locatorOptions) : base(locatorOptions)
		{
			_sitemaps = locatorOptions.Sitemaps;
		}

		/// <inheritdoc />
		public override async Task<IEnumerable<ResourceInfoBase>> LocateResourcesAsync(IServiceProvider serviceProvider,
			ResourceLocatorFilter locatorFilter)
		{
			if (locatorFilter.SkipPageResources)
				return [];

			var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
			var client = httpClientFactory.CreateClient(Constants.AnscStekeblad);
			var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
			var logger = loggerFactory.CreateLogger<SitemapPageResourceLocator>();

			List<PageResource> pageResources = [];
			var baseUrl = GetBaseUri(serviceProvider).TrimEnd('/');
			foreach (var sitemap in _sitemaps)
			{
				try
				{
					logger.LogInformation("Fetching and discovering page resources from sitemap {sitemapPath}...", sitemap);
					string xmlString = await client.GetStringAsync(baseUrl + sitemap);

					XElement siteXml = XElement.Parse(xmlString);
					var locations = siteXml.Descendants("{http://www.sitemaps.org/schemas/sitemap/0.9}loc")
						.Select(loc => loc?.Value);

					foreach (var location in locations)
					{
						if (location is null)
							continue;

						if (Uri.TryCreate(location, UriKind.Absolute, out var uri))
						{
							var pageResource = PageResourceFactory(uri.AbsolutePath);
							pageResources.Add(pageResource);
						}
					}
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Failed to fetch or process sitemap {sitemapPath}", sitemap);
				}
			}

			return pageResources;
		}
	}
}
