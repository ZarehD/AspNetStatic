using AspNetStaticContrib.Stekeblad;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetStaticContrib
{
	public static class ServiceProviderExtensions
	{
		public static IServiceCollection AddAspNetStaticContrib(this IServiceCollection services)
		{
			services.AddTransient<IStartupFilter, AspNetStaticContribStartupFilter>();
			return services;
		}

		internal class AspNetStaticContribStartupFilter : IStartupFilter
		{
			public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
			{
				return builder =>
				{
					builder.UseMiddleware<ActionDescriptorQueryingMiddleware>();
					next(builder);
				};
			}
		}
	}
}
