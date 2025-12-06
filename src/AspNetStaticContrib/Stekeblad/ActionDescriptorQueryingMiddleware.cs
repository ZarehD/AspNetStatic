using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AspNetStaticContrib.Stekeblad
{
	/// <summary>
	/// Adds an endpoint used by <see cref="ActionDescriptorPageResourceInfoProvider" />
	/// to list all Pages registered in /// <see cref="IActionDescriptorCollectionProvider" />
	/// </summary>
	internal class ActionDescriptorQueryingMiddleware
	{
		private readonly RequestDelegate _next;

		private readonly IActionDescriptorCollectionProvider _descriptorCollection;
		private readonly LinkGenerator _linkGenerator;
		private readonly ILogger<ActionDescriptorQueryingMiddleware> _logger;

		public ActionDescriptorQueryingMiddleware(RequestDelegate next,
			IActionDescriptorCollectionProvider descriptorCollection,
			LinkGenerator linkGenerator,
			ILogger<ActionDescriptorQueryingMiddleware> logger)
		{
			_next = next;
			_descriptorCollection = descriptorCollection;
			_linkGenerator = linkGenerator;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			if (context.Request.Path.Equals("/api/aspnetstatic/pageRoutes"))
			{
				var remoteIp = context.Connection.RemoteIpAddress;
				if (remoteIp is not null && !IPAddress.IsLoopback(remoteIp)
					|| !context.Request.IsAspNetStaticRequest())
				{
					context.Response.StatusCode = StatusCodes.Status403Forbidden;
					await context.Response.CompleteAsync();
					return;
				}
				var pagesDesciptors = _descriptorCollection.ActionDescriptors.Items
					.OfType<CompiledPageActionDescriptor>()
					.ToList();

				List<string> links = new(pagesDesciptors.Count);
				foreach (var page in pagesDesciptors)
				{
					string? link;
					if (string.IsNullOrEmpty(page.AreaName))
						link = _linkGenerator.GetUriByPage(context, page: page.ViewEnginePath);
					else
						link = _linkGenerator.GetUriByPage(context, page: page.ViewEnginePath, values: new { area = page.AreaName });

					if (link is null)
						_logger.LogWarning("Could not determine url to Page at {location}", page.RelativePath);
					else if (!Uri.TryCreate(link, UriKind.Absolute, out var uri))
						_logger.LogError("An invalid uri was generated for Page at {location}", page.RelativePath); // Can this happen?
					else
						links.Add(uri.AbsolutePath);
				}

				context.Response.StatusCode = 200;
				await context.Response.WriteAsJsonAsync(links.Distinct());
				return;
			}

			await _next(context);
		}
	}
}
