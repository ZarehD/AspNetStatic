using AspNetStatic;
using AspNetStaticContrib.Stekeblad.Options;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using ThrowGuard;

namespace AspNetStaticContrib.Stekeblad
{
	/// <summary>
	/// <para>
	/// A ResourceLocator is a class for retrieving a list of
	/// <see cref="ResourceInfoBase"/> using some kind of method.
	/// </para>
	/// <para>
	/// The built-in once include for example
	/// <see cref="ResourceLocators.Sitemap.SitemapPageResourceLocator"/>
	/// that locates pages by fetching your sitemap, and
	/// <see cref="ResourceLocators.ActionDescriptor.ActionDescriptorPageResourceLocator" />
	/// that locates registered Razor Pages.
	/// </para>
	/// <para>
	/// Derive from this class and create your own resource locator to
	/// locate pages or other resource by, for example, querying your database.
	/// </para>
	/// <para>
	/// Add implementations to <see cref="LocatingStaticResourcesInfoProvider"/>
	/// by calling <see cref="LocatingStaticResourcesInfoProvider.AddResourceLocator(ResourceLocatorBase)"/>
	/// or one of the helper extensions like
	/// <see cref="ResourceLocators.ActionDescriptor.ActionDescriptorLocationExtensions.AddActionDescriptorLocator(LocatingStaticResourcesInfoProvider, ResourceLocatorOptions)" />
	/// and <see cref="ResourceLocators.Sitemap.SitemapLocationExtensions.AddSitemapLocator(LocatingStaticResourcesInfoProvider, ResourceLocators.Sitemap.SitemapResourceLocatorOptions)"/>
	/// </para>
	/// </summary>
	public abstract class ResourceLocatorBase
	{
		/// <summary>
		/// Constructor for ResourceLocatorBase
		/// </summary>
		/// <param name="locatorOptions">Allow users to configure your resource locator</param>
		public ResourceLocatorBase(ResourceLocatorOptions locatorOptions)
		{
			if (locatorOptions?.PageResourceFactory is not null)
				PageResourceFactory = locatorOptions.PageResourceFactory;

			if (locatorOptions?.BinResourceFactory is not null)
				BinResourceFactory = locatorOptions.BinResourceFactory;

			if (locatorOptions?.JsResourceFactory is not null)
				JsResourceFactory = locatorOptions.JsResourceFactory;

			if (locatorOptions?.CssResourceFactory is not null)
				CssResourceFactory = locatorOptions.CssResourceFactory;
		}

		/// <summary>
		/// This method gets called by <see cref="LocatingStaticResourcesInfoProvider" />.
		/// Perform the work of locating resources for the static generator here
		/// </summary>
		/// <param name="services">Gives the resource locator access to all
		/// services registered in the application.</param>
		/// <param name="locatorFilter">
		/// Information forwarded from the resource provider, contains flags
		/// for if any resource type should be skipped.
		/// </param>
		/// <returns>All resources this resource locator has discovered.</returns>
		public abstract Task<IEnumerable<ResourceInfoBase>> LocateResourcesAsync(IServiceProvider services,
			ResourceLocatorFilter locatorFilter);

		/// <summary>
		/// Returns the protocol+domain+port the application is bound to.
		/// If multiple is set up, only one is returned.
		/// https is prioritized over http
		/// </summary>
		/// <example>https://localhost:5000/</example>
		/// <param name="serviceProvider"></param>
		/// <returns></returns>
		///<remarks>copy of <see cref="AspNetStatic.StaticGeneratorHostExtension.GetBaseUri" /></remarks>
		protected static string GetBaseUri(IServiceProvider serviceProvider)
		{
			var hostFeatures = serviceProvider.GetRequiredService<IServer>().Features;
			var serverAddresses = hostFeatures.Get<IServerAddressesFeature>();

			Throw.InvalidOpWhen(
				() => serverAddresses is null,
				$"Feature '{typeof(IServerAddressesFeature)}' is not present.");

			var hostUrls = serverAddresses!.Addresses;
			var baseUri =
				hostUrls.FirstOrDefault(x => x.StartsWith(Uri.UriSchemeHttps)) ??
				hostUrls.FirstOrDefault(x => x.StartsWith(Uri.UriSchemeHttp));

			Throw.InvalidOpWhen(
				() => baseUri is null,
				"Not possible! HTTP required" /*SR.Err_HostNotHttpService*/);

			return baseUri!;
		}

		/// <summary>
		/// Call this method instead of new'ing up <see cref="PageResource"/>s in your
		/// ResourceLocator to enable users to customize the resource information by
		/// providing a custom method in <see cref="ResourceLocatorOptions"/>
		/// </summary>
		protected Func<string, PageResource> PageResourceFactory { get; set; } = DefaultPageResourceFactory;

		/// <summary>
		/// Call this method instead of new'ing up <see cref="BinResource"/>s in your
		/// ResourceLocator to enable users to customize the resource information by
		/// providing a custom method in <see cref="ResourceLocatorOptions"/>
		/// </summary>
		protected Func<string, BinResource> BinResourceFactory { get; set; } = DefaultBinResourceFactory;

		/// <summary>
		/// Call this method instead of new'ing up <see cref="JsResource"/>s in your
		/// ResourceLocator to enable users to customize the resource information by
		/// providing a custom method in <see cref="ResourceLocatorOptions"/>
		/// </summary>
		protected Func<string, JsResource> JsResourceFactory { get; set; } = DefaultJsResourceFactory;

		/// <summary>
		/// Call this method instead of new'ing up <see cref="CssResource"/>s in your
		/// ResourceLocator to enable users to customize the resource information by
		/// providing a custom method in <see cref="ResourceLocatorOptions"/>
		/// </summary>
		protected Func<string, CssResource> CssResourceFactory { get; set; } = DefaultCssResourceFactory;

		private static PageResource DefaultPageResourceFactory(string route) => new(route);
		private static BinResource DefaultBinResourceFactory(string route) => new(route);
		private static JsResource DefaultJsResourceFactory(string route) => new(route);
		private static CssResource DefaultCssResourceFactory(string route) => new(route);
	}
}
