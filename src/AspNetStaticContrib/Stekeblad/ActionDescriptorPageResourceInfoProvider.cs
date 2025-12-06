using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace AspNetStaticContrib.Stekeblad
{
	/// <summary>
	/// Uses <see cref="Microsoft.AspNetCore.Mvc.Infrastructure.IActionDescriptorCollectionProvider" />
	/// to find all registred Pages on your site after startup and adds them for static generation.
	/// </summary>
	public class ActionDescriptorPageResourceInfoProvider : DynamicResourceInfoProviderBase
	{
		public ActionDescriptorPageResourceInfoProvider()
		{
		}

		public override async Task DiscoverResourcesAsync(IServiceProvider serviceProvider)
		{
			var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
			var client = httpClientFactory.CreateClient("AspNetStatic");

			var pageLinks = await client.GetFromJsonAsync<string[]>(
				$"{GetBaseUri(serviceProvider).TrimEnd('/')}/api/aspnetstatic/pageRoutes");

			if (pageLinks is null)
				return;

			resources.Clear();
			resources.AddRange(pageLinks.Select(x => new PageResource(x)));
		}

		// copy of AspNetStatic.StaticGeneratorHostExtension.GetBaseUri
		private static string GetBaseUri(IServiceProvider serviceProvider)
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
	}
}
