using AspNetStatic;
using AspNetStaticContrib.Stekeblad.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace AspNetStaticContrib.Stekeblad.ResourceLocators.ActionDescriptor
{
	public static class ActionDescriptorLocationExtensions
	{
		/// <summary>
		/// <para>
		/// Creates an instance of
		/// <see cref="ActionDescriptorPageResourceLocator" />
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
		/// <see cref="ResourceInfoBase">resource info</see> objects
		/// </param>
		/// <returns>The resource provider, to enable chaining of add calls</returns>
		/// <exception cref="InvalidOperationException">If resources already have been located.
		/// Resources can not be added or removed after generation has started or between updates
		/// (if update interval is configured)</exception>
		public static LocatingStaticResourcesInfoProvider AddActionDescriptorLocator(this LocatingStaticResourcesInfoProvider resourceProvider,
			ResourceLocatorOptions? locatorOptions = null)
		{
			var locator = new ActionDescriptorPageResourceLocator(locatorOptions ?? new());
			return resourceProvider.AddResourceLocator(locator);
		}
	}

	/// <summary>
	/// This resource locator discovers compiled Razor pages by processing the ASP.NET Core collection
	/// with all registered ActionDescriptors and creates <see cref="PageResource"/>s
	/// for them. It does not return any MVC-pages and does not do anything if
	/// <see cref="StaticResourcesInfoProviderBase.SkipPageResources" />
	/// on the provider is set to true.
	/// </summary>
	public class ActionDescriptorPageResourceLocator : ResourceLocatorBase
	{
		public ActionDescriptorPageResourceLocator(ResourceLocatorOptions locatorOptions) : base(locatorOptions)
		{
		}

		/// <inheritdoc />
		public override async Task<IEnumerable<ResourceInfoBase>> LocateResourcesAsync(IServiceProvider serviceProvider,
			ResourceLocatorFilter locatorFilter)
		{
			if (locatorFilter.SkipPageResources)
				return [];

			var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
			var client = httpClientFactory.CreateClient(Constants.AnscStekeblad);

			var pageLinks = await client.GetFromJsonAsync<string[]>(
				$"{GetBaseUri(serviceProvider).TrimEnd('/')}{Constants.ActionDescriptorRoute}");

			if (pageLinks is null)
				return [];

			List<PageResource> pageResources = new(pageLinks.Length);
			foreach (string link in pageLinks)
			{
				var resource = PageResourceFactory(link);
				pageResources.Add(resource);
			}
			return pageResources;
		}
	}
}
