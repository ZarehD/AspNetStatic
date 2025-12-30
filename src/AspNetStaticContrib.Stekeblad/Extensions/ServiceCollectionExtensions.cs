using AspNetStaticContrib.Stekeblad.ResourceLocators.ActionDescriptor;
using AspNetStaticContrib.Stekeblad.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace AspNetStaticContrib.Stekeblad.Extensions
{
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Registers services required for the various
		/// <see cref="ResourceLocatorBase" /> implementations
		/// in AspNetStatic.Stekeblad.
		/// Nothing is registered unless the relevant options property is set to true
		/// in <paramref name="configureOptions"/>.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="configureOptions">An action for configuring witch resource locators to register</param>
		/// <returns></returns>
		public static IServiceCollection AddAspNetStaticContribStekeblad(this IServiceCollection services,
			Action<ContribStekebladOptions> configureOptions)
		{
			ContribStekebladOptions options = new();
			configureOptions(options);

			// Both ActionDescriptor and Sitemap needs the HttpClient
			if (options.RegisterActionDescriptorResourceLocator
				|| options.RegisterSitemapResourceLocator)
			{
				services.AddHttpClient(Constants.AnscStekeblad, client =>
				{
					client.DefaultRequestHeaders.Add(HeaderNames.UserAgent, Constants.AspNetStatic);
				});

			}

			// ActionDescriptor needs it's special endpoint
			if (options.RegisterActionDescriptorResourceLocator)
				services.AddTransient<IStartupFilter, AspNetStaticContribStartupFilter>();

			return services;
		}
	}
}
