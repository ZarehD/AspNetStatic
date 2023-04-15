/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023 Zareh DerGevorkian

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
using Microsoft.Net.Http.Headers;

namespace AspNetStatic
{
	public class StaticPageFallbackMiddleware
	{
		private readonly ILogger<StaticPageFallbackMiddleware>? _logger;
		private readonly IFileSystem _fileSystem;
		private readonly RequestDelegate _next;
		private readonly IStaticPagesInfoProvider _pageInfoProvider;
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
			IStaticPagesInfoProvider pageInfoProvider,
			IWebHostEnvironment environment,
			IOptions<StaticPageFallbackMiddlewareOptions>? optionsAccessor)
		{
			this._logger = logger;

			this._fileSystem = Throw.IfNull(fileSystem, nameof(fileSystem));
			this._next = Throw.IfNull(next, nameof(next));
			this._pageInfoProvider = Throw.IfNull(pageInfoProvider, nameof(pageInfoProvider));
			Throw.IfNull(environment, nameof(environment));
			var options = optionsAccessor?.Value ?? new StaticPageFallbackMiddlewareOptions();

			this._haveStaticPages = this._pageInfoProvider.Pages.Any();
			this._pageFileExtension = this._pageInfoProvider.PageFileExtension.EnsureStartsWith(".");
			this._defaultFileName = $"{this._pageInfoProvider.DefaultFileName}{this._pageFileExtension}";
			this._exclusions = this._pageInfoProvider.DefaultFileExclusions;

			this._alwaysDefaultFile = options.AlwaysDefaultFile;
			this._ignoreOutFilePathname = options.IgnoreOutFilePathname;

			this._webRoot = environment.WebRootPath;

			this._logger?.Configuration(
				this._pageInfoProvider.Pages.Count(),
				this._alwaysDefaultFile,
				this._ignoreOutFilePathname,
				this._webRoot,
				this._defaultFileName,
				this._pageFileExtension,
				this._exclusions);
		}

		public async Task InvokeAsync(HttpContext ctx)
		{
			if (this._haveStaticPages)
			{
				var isAspNetStatic =
					ctx.Request.Headers.TryGetValue(HeaderNames.UserAgent, out var ua) &&
					ua.Contains(Consts.AspNetStatic);

				if (!isAspNetStatic)
				{
					var path = ctx.Request.Path.Value.EnsureStartsWith(Consts.FwdSlash);
					var query = ctx.Request.QueryString.Value.EnsureStartsWith('?', true);
					var page = this._pageInfoProvider.Pages.GetPageForUrl($"{path}{query}");

					if (page is not null)
					{
						if (!this._ignoreOutFilePathname &&
							!string.IsNullOrWhiteSpace(page.OutFile))
						{
							var physicalPath =
								Path.Combine(this._webRoot,
								page.OutFile.EnsureNotStartsWith(
									Path.DirectorySeparatorChar));

							if (this._fileSystem.File.Exists(physicalPath))
							{
								var newPath = page.OutFile.Replace(
									Path.DirectorySeparatorChar, Consts.FwdSlash)
									.EnsureStartsWith(Consts.FwdSlash);

								this._logger?.ProcessedRoute(path, newPath);

								ctx.Request.Path = newPath;
							}
							else
							{
								this._logger?.NoFileAtProposedRoute(path, "N/A", physicalPath);
							}
						}
						else
						{
							var hasExtension = Path.HasExtension(path);
							var endsWithSlash = path.EndsWith(Consts.FwdSlash);
							var alwaysDefault = this._alwaysDefaultFile;

							this._logger?.ProcessingRoute(path, hasExtension, endsWithSlash, alwaysDefault);

							var newPath =
								(endsWithSlash || (!endsWithSlash && !hasExtension && alwaysDefault))
								? path.ToDefaultFileFallback(
									this._exclusions,
									this._defaultFileName,
									this._pageFileExtension)
								: (!endsWithSlash && !hasExtension && !alwaysDefault)
								? path + this._pageFileExtension
								: string.Empty
								;

							if (!string.IsNullOrEmpty(newPath))
							{
								var physicalPath =
									Path.Combine(this._webRoot, newPath
									.Replace(Consts.BakSlash, Path.DirectorySeparatorChar)
									.Replace(Consts.FwdSlash, Path.DirectorySeparatorChar)
									.EnsureNotStartsWith(Path.DirectorySeparatorChar));

								if (this._fileSystem.File.Exists(physicalPath))
								{
									this._logger?.ProcessedRoute(path, newPath);
									ctx.Request.Path = newPath;
								}
								else
								{
									this._logger?.NoFileAtProposedRoute(path, newPath, physicalPath);
								}
							}
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
		///		in <see cref="PageInfo.OutFile"/>, and instead 
		///		use apply the usual rules to determine the fallback route; 
		///		otherwise, if <see cref="PageInfo.OutFile"/> 
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

	public static partial class StaticPageFallbackMiddlewareLoggerExtensions
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
			bool alwaysDefault) =>
			logger.Imp_ProcessingRoute(
				route,
				hasExtension,
				endsWithSlash,
				alwaysDefault);

		[LoggerMessage(
			EventId = 1002, EventName = "ProcessingRoute", Level = LogLevel.Trace,
			Message = "StaticPageFallbackMiddleware: Processing route > Route = {Route}, HasExtension = {HasExtension}, EndsWithSlash = {EndsWithSlash}, AlwaysDefault = {AlwaysDefault}")]
		private static partial void Imp_ProcessingRoute(
			this ILogger<StaticPageFallbackMiddleware> logger,
			string route,
			bool hasExtension,
			bool endsWithSlash,
			bool alwaysDefault);

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

		#region 1004 - NoFileAtProposedRoute

		public static void NoFileAtProposedRoute(
			this ILogger<StaticPageFallbackMiddleware> logger,
			string origRoute,
			string newRoute,
			string physicalPath) =>
			logger.Imp_NoFileAtProposedRoute(
				origRoute,
				newRoute,
				physicalPath);

		[LoggerMessage(
			EventId = 1004, EventName = "NoFileAtProposedRoute", Level = LogLevel.Warning,
			Message = "StaticPageFallbackMiddleware: No such file at proposed route > OrigRoute = {OrigRoute}, NewRoute = {NewRoute}, PhysicalPath = {PhysicalPath}")]
		private static partial void Imp_NoFileAtProposedRoute(
			this ILogger<StaticPageFallbackMiddleware> logger,
			string origRoute,
			string newRoute,
			string physicalPath);

		#endregion
	}

	#endregion
}
