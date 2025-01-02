/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023-2025 Zareh DerGevorkian

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License. 
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
--------------------------------------------------------------------------------------------------------------------------------*/

using System.IO.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetStatic
{
	internal sealed class StaticPageFallbackMiddleware
	{
		private readonly ILogger<StaticPageFallbackMiddleware>? _logger;
		private readonly IFileSystem _fileSystem;
		private readonly RequestDelegate _next;
		private readonly IStaticResourcesInfoProvider _resourceInfoProvider;
		private readonly bool _haveStaticPages;
		private readonly bool _alwaysDefaultFile;
		private readonly bool _ignoreOutFilePathname;
		private readonly string _webRoot;
		private readonly string[] _exclusions;
		private readonly string _defaultFileName;
		private readonly string _pageFileExtension;

		public StaticPageFallbackMiddleware(
			ILogger<StaticPageFallbackMiddleware>? logger,
			IFileSystem fileSystem,
			RequestDelegate next,
			IStaticResourcesInfoProvider resourceInfoProvider,
			IWebHostEnvironment environment,
			IOptions<StaticPageFallbackMiddlewareOptions>? optionsAccessor)
		{
			this._logger = logger;

			this._fileSystem = Throw.IfNull(fileSystem);
			this._next = Throw.IfNull(next);
			this._resourceInfoProvider = Throw.IfNull(resourceInfoProvider);
			Throw.IfNull(environment);
			var options = optionsAccessor?.Value ?? new StaticPageFallbackMiddlewareOptions();

			this._haveStaticPages = this._resourceInfoProvider.PageResources.Any();
			this._pageFileExtension = this._resourceInfoProvider.PageFileExtension.EnsureStartsWith(".");
			this._defaultFileName = $"{this._resourceInfoProvider.DefaultFileName}{this._pageFileExtension}";
			this._exclusions = this._resourceInfoProvider.DefaultFileExclusions;

			this._alwaysDefaultFile = options.AlwaysDefaultFile;
			this._ignoreOutFilePathname = options.IgnoreOutFilePathname;

			this._webRoot = environment.WebRootPath;

			this._logger?.Configuration(
				this._resourceInfoProvider.PageResources.Count(),
				this._alwaysDefaultFile,
				this._ignoreOutFilePathname,
				this._webRoot,
				this._defaultFileName,
				this._pageFileExtension,
				this._exclusions);
		}

		public async Task InvokeAsync(HttpContext ctx)
		{
			if (!this._haveStaticPages || ctx.Request.IsAspNetStaticRequest())
			{
				await this._next(ctx).ConfigureAwait(false);
				return;
			}

			var path = ctx.Request.Path.Value.EnsureStartsWith(Consts.FwdSlash);
			var query = ctx.Request.QueryString.Value.EnsureStartsWith('?', true);
			var page = (PageResource?) this._resourceInfoProvider.PageResources.GetResourceForUrl($"{path}{query}");

			var hasExtension = Path.HasExtension(path);
			var endsWithFSlash = path.EndsWith(Consts.FwdSlash);
			var alwaysDefault = this._alwaysDefaultFile;

			this._logger?.ProcessingRoute(path, hasExtension, endsWithFSlash, alwaysDefault, page is not null);

			if (page is not null)
			{
				var isLeaf = !endsWithFSlash && !hasExtension;
				var newPath =
					!this._ignoreOutFilePathname && !string.IsNullOrWhiteSpace(page.OutFile)
					? page.OutFile
						.Replace(Path.DirectorySeparatorChar, Consts.FwdSlash)
						.EnsureStartsWith(Consts.FwdSlash)
					: (endsWithFSlash || (isLeaf && alwaysDefault))
						? path.ToDefaultFileFallback(this._exclusions, this._defaultFileName, this._pageFileExtension)
						: isLeaf && !alwaysDefault
							? path + this._pageFileExtension
							: string.Empty
					;

				var filePath =
					Path.Combine(
						this._webRoot, newPath.ToFileSysPath()
						.EnsureNotStartsWith(Path.DirectorySeparatorChar));

				if (this._fileSystem.File.Exists(filePath))
				{
					this._logger?.ProcessedRoute(path, newPath);
					ctx.Request.Path = newPath;
				}
				else
				{
					this._logger?.NoStaticFileAtProposedRoute(path, newPath, filePath);
				}

				#region OBSOLETE

				//if (!this._ignoreOutFilePathname && !string.IsNullOrWhiteSpace(page.OutFile))
				//{ // user specified outfile...
				//	var outFile = page.OutFile.ToFileSysPath().EnsureNotStartsWith(Path.DirectorySeparatorChar);
				//	var physicalPath = Path.Combine(this._webRoot, outFile);

				//	if (this._fileSystem.File.Exists(physicalPath))
				//	{
				//		var newPath = outFile
				//			.Replace(Path.DirectorySeparatorChar, Consts.FwdSlash)
				//			.EnsureStartsWith(Consts.FwdSlash);

				//		this._logger?.ProcessedRoute(RouteHandledBy.UserOutfile, path, newPath);

				//		ctx.Request.Path = newPath;
				//	}
				//	else
				//	{
				//		this._logger?.NoStaticFileAtProposedRoute(path, "N/A", physicalPath);
				//	}
				//}
				//else
				//{ // generated outfile pathname...
				//	var newPath =
				//		(endsWithFSlash || (!endsWithFSlash && !hasExtension && alwaysDefault))
				//		? path.ToDefaultFileFallback(this._exclusions, this._defaultFileName, this._pageFileExtension)
				//		: (!endsWithFSlash && !hasExtension && !alwaysDefault)
				//		? path + this._pageFileExtension
				//		: string.Empty
				//		;

				//	if (!string.IsNullOrEmpty(newPath))
				//	{
				//		var physicalPath =
				//			Path.Combine(
				//				this._webRoot, newPath.ToFileSysPath()
				//				.EnsureNotStartsWith(Path.DirectorySeparatorChar));

				//		if (this._fileSystem.File.Exists(physicalPath))
				//		{
				//			this._logger?.ProcessedRoute(RouteHandledBy.GeneratedOutfile, path, newPath);
				//			ctx.Request.Path = newPath;
				//		}
				//		else
				//		{
				//			this._logger?.NoStaticFileAtProposedRoute(path, newPath, physicalPath);
				//		}
				//	}
				//}

				#endregion
			}
			else
			{
				var isForStaticPage = hasExtension && path.EndsWith(
					this._pageFileExtension) || path.EndsWith(".htm") || path.EndsWith(".html");

				if (isForStaticPage)
				{
					this._logger?.CheckingForReverseFallback(path);

					var filePath =
						Path.Combine(
							this._webRoot, path.ToFileSysPath()
							.EnsureNotStartsWith(Path.DirectorySeparatorChar));

					this._logger?.ReverseFallbackDebug(filePath);

					if (!this._fileSystem.File.Exists(filePath))
					{
						page = this._resourceInfoProvider.PageResources
							.GetPageResourceForStaticPage(
								new(this._alwaysDefaultFile,
								this._defaultFileName,
								this._pageFileExtension,
								this._exclusions),
								path);

						if (page is not null)
						{
							var newPath = page.Url.EnsureStartsWith(Consts.FwdSlash);
							this._logger?.ProcessedRoute(path, newPath);
							ctx.Request.Path = newPath;
						}
						else
						{
							this._logger?.NoPageResourceForRequestedFile(path);
						}
					}
				}
			}

			await this._next(ctx).ConfigureAwait(false);
		}
	}

	public class StaticPageFallbackMiddlewareOptions
	{
		/// <summary>
		///		Gets a value that, when true, indicates that when 
		///		<see cref="CreateDefaultFile"/> is <c>True</c>, 
		///		a default file should only be created if the route 
		///		does not specify a page name (/segment/page/) and do 
		///		an html fallback (/segment/page to /segment/page.html).
		///		When false, a default file should be created in both 
		///		scenarios (/segment/page/ and /segment/page).
		/// </summary>
		/// <remarks>
		///		Default is <c>False</c>.
		/// </remarks>
		public bool AlwaysDefaultFile { get; set; }

		/// <summary>
		///		Gets or sets a value that, when true, indicates that 
		///		the fallback route should ignore the value specified 
		///		in <see cref="PageResource.OutFile"/>, and instead 
		///		use apply the usual rules to determine the fallback route; 
		///		otherwise, if <see cref="PageResource.OutFile"/> 
		///		specifies a file pathname, it should be used as 
		///		the fallback route.
		/// </summary>
		/// <remarks>
		///		Default is <c>False</c>.
		/// </remarks>
		public bool IgnoreOutFilePathname { get; set; }
	}

	public static class StaticPageFallbackMiddlewareExtensions
	{
		public static IServiceCollection AddStaticPageFallback(
			this IServiceCollection services,
			Action<StaticPageFallbackMiddlewareOptions>? configAction = default)
		{
			services.AddTransient<IFileSystem, FileSystem>();

			var options = new StaticPageFallbackMiddlewareOptions();
			configAction?.Invoke(options);
			services.Configure<StaticPageFallbackMiddlewareOptions>(
				cfg =>
				{
					cfg.AlwaysDefaultFile = options.AlwaysDefaultFile;
					cfg.IgnoreOutFilePathname = options.IgnoreOutFilePathname;
				});

			return services;
		}

		public static IApplicationBuilder UseStaticPageFallback(
			this IApplicationBuilder builder) =>
			builder.UseMiddleware<StaticPageFallbackMiddleware>();
	}


	#region Logger Messages...

	internal static partial class StaticPageFallbackMiddlewareLoggerExtensions
	{
		#region 1001 - Configuration

		public static void Configuration(
			this ILogger<StaticPageFallbackMiddleware> logger,
			int pageCount,
			bool alwaysDefaultFile,
			bool ignoreOutFilePathname,
			string webroot,
			string defaultFileName,
			string pageFileExtension,
			string[] defaultFileExclusions) =>
			logger.Imp_Configuration(
				pageCount,
				alwaysDefaultFile,
				ignoreOutFilePathname,
				webroot,
				defaultFileName,
				pageFileExtension,
				defaultFileExclusions);

		[LoggerMessage(
			EventId = 1001, EventName = "Configuration", Level = LogLevel.Information,
			Message = "StaticPageFallbackMiddleware: Configuration > PageCount = {PageCount}, AlwaysDefaultFile = {AlwaysDefaultFile}, IgnoreOutFilePathname = {IgnoreOutFilePathname}, DefaultFileName = {DefaultFileName}, PageFileExtension = {PageFileExtension}, DefaultFileExclusions = {DefaultFileExclusions}, WebRoot = {WebRoot}")]
		private static partial void Imp_Configuration(
			this ILogger<StaticPageFallbackMiddleware> logger,
			int pageCount,
			bool alwaysDefaultFile,
			bool ignoreOutFilePathname,
			string webroot,
			string defaultFileName,
			string pageFileExtension,
			string[] defaultFileExclusions);

		#endregion

		#region 1002 - ProcessingRoute

		public static void ProcessingRoute(
			this ILogger<StaticPageFallbackMiddleware> logger,
			string route,
			bool hasExtension,
			bool endsWithSlash,
			bool alwaysDefault,
			bool foundPageResource) =>
			logger.Imp_ProcessingRoute(
				route,
				hasExtension,
				endsWithSlash,
				alwaysDefault,
				foundPageResource);

		[LoggerMessage(
			EventId = 1002, EventName = "ProcessingRoute", Level = LogLevel.Trace,
			Message = "StaticPageFallbackMiddleware: Processing route > Route = {Route}, HasExtension = {HasExtension}, EndsWithSlash = {EndsWithSlash}, AlwaysDefault = {AlwaysDefault}, FoundPageResource = {FoundPageResource}")]
		private static partial void Imp_ProcessingRoute(
			this ILogger<StaticPageFallbackMiddleware> logger,
			string route,
			bool hasExtension,
			bool endsWithSlash,
			bool alwaysDefault,
			bool foundPageResource);

		#endregion

		#region 1003 - ProcessedRoute

		public static void ProcessedRoute(
			this ILogger<StaticPageFallbackMiddleware> logger,
			string origRoute,
			string newRoute) =>
			logger.Imp_ProcessedRoute(
				origRoute,
				newRoute);

		[LoggerMessage(
			EventId = 1003, EventName = "ProcessedRoute", Level = LogLevel.Information,
			Message = "StaticPageFallbackMiddleware: Processed route > OrigRoute = {OrigRoute}, NewRoute = {NewRoute}")]
		private static partial void Imp_ProcessedRoute(
			this ILogger<StaticPageFallbackMiddleware> logger,
			string origRoute,
			string newRoute);

		#endregion

		#region 1004 - NoStaticFileAtProposedRoute

		public static void NoStaticFileAtProposedRoute(
			this ILogger<StaticPageFallbackMiddleware> logger,
			string origRoute,
			string newRoute,
			string physicalPath) =>
			logger.Imp_NoStaticFileAtProposedRoute(
				origRoute,
				newRoute,
				physicalPath);

		[LoggerMessage(
			EventId = 1004, EventName = "NoStaticFileAtProposedRoute", Level = LogLevel.Warning,
			Message = "StaticPageFallbackMiddleware: No static file at proposed route > OrigRoute = {OrigRoute}, NewRoute = {NewRoute}, PhysicalPath = {PhysicalPath}")]
		private static partial void Imp_NoStaticFileAtProposedRoute(
			this ILogger<StaticPageFallbackMiddleware> logger,
			string origRoute,
			string newRoute,
			string physicalPath);

		#endregion

		#region 1008 - CheckingForReverseFallback

		public static void CheckingForReverseFallback(
			this ILogger<StaticPageFallbackMiddleware> logger,
			string origRoute) =>
			logger.Imp_CheckingForReverseFallback(
				origRoute);

		[LoggerMessage(
			EventId = 1008, EventName = "CheckingForReverseFallback", Level = LogLevel.Trace,
			Message = "StaticPageFallbackMiddleware: Checking need for reverse fallback > Route = {Route}")]
		private static partial void Imp_CheckingForReverseFallback(
			this ILogger<StaticPageFallbackMiddleware> logger,
			string route);

		#endregion

		#region 1009 - ReverseFallbackDebug

		public static void ReverseFallbackDebug(
			this ILogger<StaticPageFallbackMiddleware> logger,
			string filePath) =>
			logger.Imp_ReverseFallbackDebug(
				filePath);

		[LoggerMessage(
			EventId = 1009, EventName = "ReverseFallbackDebug", Level = LogLevel.Debug,
			Message = "StaticPageFallbackMiddleware: Reverse fallback criteria > FilePath = {FilePath}")]
		private static partial void Imp_ReverseFallbackDebug(
			this ILogger<StaticPageFallbackMiddleware> logger,
			string filePath);

		#endregion

		#region 1012 - NoPageResourceForRequestedFile

		public static void NoPageResourceForRequestedFile(
			this ILogger<StaticPageFallbackMiddleware> logger,
			string route) =>
			logger.Imp_NoPageResourceForRequestedFile(
				route);

		[LoggerMessage(
			EventId = 1012, EventName = "NoPageResourceForRequestedFile", Level = LogLevel.Trace,
			Message = "StaticPageFallbackMiddleware: Static page not found and no fallback page resource > Route = {Route}")]
		private static partial void Imp_NoPageResourceForRequestedFile(
			this ILogger<StaticPageFallbackMiddleware> logger,
			string route);

		#endregion
	}

	#endregion
}
