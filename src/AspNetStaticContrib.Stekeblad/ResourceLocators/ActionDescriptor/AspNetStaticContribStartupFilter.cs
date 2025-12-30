using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace AspNetStaticContrib.Stekeblad.ResourceLocators.ActionDescriptor
{
	/// <summary>
	/// Adds the <see cref="ActionDescriptorQueryingMiddleware"/> middleware
	/// to the beginning of the request pipeline.
	/// </summary>
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
