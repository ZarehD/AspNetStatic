using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace AspNetStatic
{
	public static class ServiceProviderExtensions
	{
		public static IServiceCollection AddAspNetStatic(this IServiceCollection services)
		{
			services.AddHttpClient(Consts.AspNetStatic, client =>
			{
				client.DefaultRequestHeaders.Add(HeaderNames.UserAgent, Consts.AspNetStatic);
			});

			return services;
		}
	}
}
