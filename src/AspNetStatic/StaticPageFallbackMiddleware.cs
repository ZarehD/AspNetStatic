using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AspNetStatic
{
	public class StaticPageFallbackMiddleware
	{
		private readonly ILogger<StaticPageFallbackMiddleware>? _logger;
		private readonly RequestDelegate _next;
		private readonly IStaticPagesInfoProvider _pageInfoProvider;
		private readonly bool _haveStaticPages;
		private readonly bool _alwaysDefaultFile;
		private readonly string _webRoot;
		private readonly string[] _exclusions;
		private readonly string _defaultFileName;
		private readonly string _pageFileExtension;


		public StaticPageFallbackMiddleware(
			ILogger<StaticPageFallbackMiddleware>? logger,
			RequestDelegate next,
			IStaticPagesInfoProvider pageInfoProvider,
			IWebHostEnvironment environment,
			StaticPageFallbackMiddlewareOptions? options)
		{
			this._logger = logger;

			this._next = next ??
				throw new ArgumentNullException(
					nameof(next));

			this._pageInfoProvider =
				pageInfoProvider ??
				throw new ArgumentNullException(
					nameof(pageInfoProvider));

			options ??= new StaticPageFallbackMiddlewareOptions();

			this._haveStaticPages = this._pageInfoProvider.Pages.Any();
			this._alwaysDefaultFile = options.AlwaysDefaultFile;
			this._exclusions = this._pageInfoProvider.DefaultFileExclusions;
			this._pageFileExtension = this._pageInfoProvider.PageFileExtension.AssureStartsWith(".");
			this._defaultFileName = $"{this._pageInfoProvider.DefaultFileName}{this._pageFileExtension}";

			this._webRoot = environment.WebRootPath;

			this._logger?.Configuration(
				this._pageInfoProvider.Pages.Count(),
				this._alwaysDefaultFile,
				this._webRoot,
				this._defaultFileName,
				this._pageFileExtension,
				this._exclusions);
		}


		public async Task InvokeAsync(HttpContext ctx)
		{
			if (this._haveStaticPages)
			{
				var path = ctx.Request.Path.Value.AssureStartsWith(RouteConsts.FwdSlash);

				if (this._pageInfoProvider.Pages.IncludesRoute(path))
				{
					var hasExtension = Path.HasExtension(path);
					var endsWithSlash = path.EndsWith(RouteConsts.FwdSlash);
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
							.Replace(RouteConsts.BakSlash, Path.DirectorySeparatorChar)
							.Replace(RouteConsts.FwdSlash, Path.DirectorySeparatorChar)
							.AssureNotStartsWith(Path.DirectorySeparatorChar));

						if (File.Exists(physicalPath))
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
	}


	public static class StaticPageFallbackMiddlewareExtensions
	{
		public static IApplicationBuilder UseStaticPageFallback(
			this IApplicationBuilder builder,
			Action<StaticPageFallbackMiddlewareOptions>? configAction = default)
		{
			var options = new StaticPageFallbackMiddlewareOptions();

			configAction?.Invoke(options);

			return builder.UseMiddleware<StaticPageFallbackMiddleware>(options);
		}
	}


	public static partial class StaticPageFallbackMiddlewareLoggerExtensions
	{
		#region 1001 - Configuration

		public static void Configuration(
			this ILogger<StaticPageFallbackMiddleware> logger,
			int pageCount,
			bool alwaysDefaultFile,
			string webroot,
			string defaultFileName,
			string pageFileExtension,
			string[] defaultFileExclusions) =>
			logger.Imp_Configuration(
				pageCount,
				alwaysDefaultFile,
				webroot,
				defaultFileName,
				pageFileExtension,
				defaultFileExclusions);

		[LoggerMessage(
			EventId = 1001, EventName = "Configuration", Level = LogLevel.Information,
			Message = "StaticPageFallbackMiddleware: Configuration > PageCount = {PageCount}, AlwaysDefaultFile = {AlwaysDefaultFile}, DefaultFileName = {DefaultFileName}, PageFileExtension = {PageFileExtension}, DefaultFileExclusions = {DefaultFileExclusions}, WebRoot = {WebRoot}")]
		private static partial void Imp_Configuration(
			this ILogger<StaticPageFallbackMiddleware> logger,
			int pageCount,
			bool alwaysDefaultFile,
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
}
