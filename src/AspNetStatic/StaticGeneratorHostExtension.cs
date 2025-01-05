/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023-2025 Zareh DerGevorkian

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License. 
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
--------------------------------------------------------------------------------------------------------------------------------*/

#define USE_PERIODIC_TIMER

using System.IO.Abstractions;
using AspNetStatic.Optimizer;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using WebMarkupMin.Core;

namespace AspNetStatic
{
	public static class StaticGeneratorHostExtension
	{
		/// <summary>
		///		Registers an action that will generates static pages when 
		///		the application is done loading (is ready to serve pages), 
		///		and periodically thereafter.
		/// </summary>
		/// <param name="host">An instance of the AspNetCore app host.</param>
		/// <param name="destinationRoot">
		///		The path to the root folder where generated static page 
		///		files (and subfolders) will be placed.
		/// </param>
		/// <param name="exitWhenDone">
		///		Specifies whether to exit the app (gracefully shut the web app down) 
		///		after generating the static files.
		/// </param>
		/// <param name="alwaysDefaultFile">
		///		<para>
		///			Specifies whether to always create default files for pages 
		///			(true) even if a route specifies a page name, or an html file 
		///			bearing the page name (false).
		///		</para>
		///		<para>
		///			Does not affect routes that end with a trailing forward slash.
		///			A default file will always be generated for such routes.
		///		</para>
		///		<para>
		///			Whereas /page/ will always produce /page/index.html, /page will 
		///			produce /page/index.html (true) or /page.html (false).
		///		</para>
		/// </param>
		/// <param name="dontUpdateLinks">
		///		<para>
		///			Indicates, when true, that the href value of [a] and [area] 
		///			HTML tags should not be modofied to refer to the generated 
		///			static pages.
		///		</para>
		///		<para>
		///			Href values will be modified such that a value of /page is 
		///			converted to /page.html or /page/index.html depending on 
		///			<paramref name="alwaysDefaultFile"/>.
		///		</para>
		/// </param>
		/// <param name="dontOptimizeContent">
		///		<para>
		///			Specifies whether to omit optimizing the content of generated static fiels.
		///		</para>
		///		<para>
		///			By default, when this parameter is <c>false</c>, content of the generated 
		///			static file will be optimized. Specify <c>true</c> to omit the optimizations.
		///		</para>
		/// </param>
		/// <param name="regenerationInterval">
		///		Specifies whether periodic re-generation is enabled (non-null value),
		///		and the interval between re-generation events.
		/// </param>
		/// <param name="httpTimeoutSeconds">
		///		The HttpClient request timeout (in seconds) while fetching page content.
		/// </param>
		public static void GenerateStaticContent(
			this IHost host,
			string destinationRoot,
			bool exitWhenDone = default,
			bool alwaysDefaultFile = default,
			bool dontUpdateLinks = default,
			bool dontOptimizeContent = default,
			TimeSpan? regenerationInterval = default,
			ulong httpTimeoutSeconds = c_DefaultHttpTimeoutSeconds)
		{
			Throw.IfNull(host);
			Throw.IfNullOrWhitespace(destinationRoot);

			var fileSystem = host.Services.GetService<IFileSystem>() ?? new FileSystem();

			Throw.DirectoryNotFoundWhen(
				() => !fileSystem.Directory.Exists(destinationRoot),
				SR.Err_InvalidDestinationRoot);

			var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
			var logger = loggerFactory.CreateLogger(nameof(StaticGeneratorHostExtension));

			var pageUrlProvider = host.Services.GetRequiredService<IStaticResourcesInfoProvider>();

			if (!pageUrlProvider.PageResources.Any())
			{
				logger.NoPagesToProcess();
				return;
			}

			var optimizerSelector = GetOptimizerSelector(host.Services, dontOptimizeContent);

			var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

			lifetime.ApplicationStopping.Register(_appShutdown.Cancel);

			lifetime.ApplicationStarted.Register(
				async () =>
				{
					try
					{
						_httpClient.BaseAddress = new Uri(GetBaseUri(host));
						_httpClient.Timeout = TimeSpan.FromSeconds(httpTimeoutSeconds);
						_httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, Consts.AspNetStatic);

						var generatorConfig =
							new StaticGeneratorConfig(
								pageUrlProvider.Resources,
								destinationRoot,
								alwaysDefaultFile,
								!dontUpdateLinks,
								pageUrlProvider.DefaultFileName,
								pageUrlProvider.PageFileExtension.EnsureStartsWith('.'),
								pageUrlProvider.DefaultFileExclusions,
								!dontOptimizeContent, optimizerSelector,
								pageUrlProvider.SkipPageResources,
								pageUrlProvider.SkipCssResources,
								pageUrlProvider.SkipJsResources,
								pageUrlProvider.SkipBinResources);

						logger.RegenerationConfig(regenerationInterval);
						var doPeriodicRefresh = regenerationInterval is not null;

#if USE_PERIODIC_TIMER
						if (!_appShutdown.IsCancellationRequested)
						{
							if (doPeriodicRefresh) _timer = new(regenerationInterval!.Value);
							do
							{
								await StaticGenerator.Execute(
									generatorConfig,
									_httpClient, fileSystem,
									loggerFactory, _appShutdown.Token)
								.ConfigureAwait(false);
							}
							while (doPeriodicRefresh && await _timer.WaitForNextTickAsync(_appShutdown.Token).ConfigureAwait(false));
						}

						if (exitWhenDone && !doPeriodicRefresh && !_appShutdown.IsCancellationRequested)
						{
							logger.Exiting();
							await Task.Delay(500).ConfigureAwait(false);
							await host.StopAsync().ConfigureAwait(false);
						}
#else
						await StaticPageGenerator.Execute(
							generatorConfig,
							_httpClient, fileSystem,
							loggerFactory, _appShutdown.Token)
							.ConfigureAwait(false);

						if (doPeriodicRefresh && !_appShutdown.IsCancellationRequested)
						{
							_timer.Interval = regenerationInterval!.Value.TotalMilliseconds;
							_timer.Elapsed +=
								async (s, e) =>
								{
									await StaticPageGenerator.Execute(
										generatorConfig,
										_httpClient, fileSystem,
										loggerFactory, _appShutdown.Token)
										.ConfigureAwait(false);

									if (_appShutdown.IsCancellationRequested)
									{
										_timer.Stop();
									}
								};
							_timer.Start();
						}

						if (_appShutdown.IsCancellationRequested)
						{
							_timer.Stop();
						}
						else if (exitWhenDone && !doPeriodicRefresh)
						{
							logger.Exiting();
							await Task.Delay(500).ConfigureAwait(false);
							await host.StopAsync().ConfigureAwait(false);
						}
#endif
					}
					catch (OperationCanceledException) { }
					catch (Exception ex)
					{
						logger.Exception(ex);
					}
				});
		}


		/// <summary>
		///		Generates static pages (now, upon execution).
		/// </summary>
		/// <remarks>
		///		NOTE: Call this method only when the application 
		///		has 'started' and is ready to serve pages.
		/// </remarks>
		/// <param name="host">An instance of the AspNetCore app host.</param>
		/// <param name="destinationRoot">
		///		The path to the root folder where generated static page 
		///		files (and subfolders) will be placed.
		/// </param>
		/// <param name="alwaysDefaultFile">
		///		<para>
		///			Specifies whether to always create default files for pages 
		///			(true) even if a route specifies a page name, or an html file 
		///			bearing the page name (false).
		///		</para>
		///		<para>
		///			Does not affect routes that end with a trailing forward slash.
		///			A default file will always be generated for such routes.
		///		</para>
		///		<para>
		///			Whereas /page/ will always produce /page/index.html, /page will 
		///			produce /page/index.html (true) or /page.html (false).
		///		</para>
		/// </param>
		/// <param name="dontUpdateLinks">
		///		<para>
		///			Indicates, when true, that the href value of [a] and [area] 
		///			HTML tags should not be modofied to refer to the generated 
		///			static pages.
		///		</para>
		///		<para>
		///			Href values will be modified such that a value of /page is 
		///			converted to /page.html or /page/index.html depending on 
		///			<paramref name="alwaysDefaultFile"/>.
		///		</para>
		/// </param>
		/// <param name="dontOptimizeContent">
		///		<para>
		///			Specifies whether to omit optimizing the content of generated static fiels.
		///		</para>
		///		<para>
		///			By default, when this parameter is <c>false</c>, content of the generated 
		///			static file will be optimized. Specify <c>true</c> to omit the optimizations.
		///		</para>
		/// </param>
		/// <param name="httpClientName">
		///		Optional. The name of a configured HTTP client to use for fetching pages.
		/// </param>
		/// <param name="httpTimeoutSeconds">
		///		<para>
		///			The HttpClient request timeout (in seconds) while fetching page content.
		///		</para>
		///		<para>
		///			NOTE: Applies only when the HttpClient instance is created locally, or 
		///			when the timeout for the acquired instance is <see cref="Timespan.Zero"/>.
		///		</para>
		/// </param>
		/// <param name="ct">The object to monitor for cancellation requests.</param>
		/// <returns>
		///		An object representing the async operation that will return 
		///		a boolean True if the operation succeeded, or False otherwise.
		/// </returns>
		public static async Task<bool> GenerateStaticContentNow(
			this IHost host,
			string destinationRoot,
			bool alwaysDefaultFile = default,
			bool dontUpdateLinks = default,
			bool dontOptimizeContent = default,
			string? httpClientName = default,
			ulong httpTimeoutSeconds = c_DefaultHttpTimeoutSeconds,
			CancellationToken ct = default)
		{
			Throw.IfNull(host);
			Throw.IfNullOrWhitespace(destinationRoot);

			var fileSystem = host.Services.GetService<IFileSystem>() ?? new FileSystem();

			Throw.DirectoryNotFoundWhen(
				() => !fileSystem.Directory.Exists(destinationRoot),
				SR.Err_InvalidDestinationRoot);

			var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
			var logger = loggerFactory.CreateLogger(nameof(StaticGeneratorHostExtension));

			var pageUrlProvider = host.Services.GetRequiredService<IStaticResourcesInfoProvider>();

			if (!pageUrlProvider.PageResources.Any())
			{
				logger.NoPagesToProcess();
				return false;
			}

			var optimizerSelector = GetOptimizerSelector(host.Services, dontOptimizeContent);

			var httpClient = GetHttpClient(host, httpClientName, httpTimeoutSeconds);

			try
			{
				await StaticGenerator.Execute(
					new StaticGeneratorConfig(
						pageUrlProvider.Resources,
						destinationRoot,
						alwaysDefaultFile,
						!dontUpdateLinks,
						pageUrlProvider.DefaultFileName,
						pageUrlProvider.PageFileExtension.EnsureStartsWith('.'),
						pageUrlProvider.DefaultFileExclusions,
						!dontOptimizeContent, optimizerSelector,
						pageUrlProvider.SkipPageResources,
						pageUrlProvider.SkipCssResources,
						pageUrlProvider.SkipJsResources,
						pageUrlProvider.SkipBinResources),
					httpClient,
					fileSystem,
					loggerFactory,
					ct).ConfigureAwait(false);

				return true;
			}
			catch (OperationCanceledException) { }
			catch (Exception ex)
			{
				logger.Exception(ex);
			}

			return false;
		}


		/// <summary>
		///		Generates a static file for the specified page.
		/// </summary>
		/// <remarks>
		///		NOTE: Call this method only when the application 
		///		has 'started' and is ready to serve pages.
		/// </remarks>
		/// <param name="host">An instance of the AspNetCore app host.</param>
		/// <param name="pageUrl">
		///		<para>
		///			The URL for the page to be generated.
		///		</para>
		///		<para>
		///			The specified URL must uniquely identify a <see cref="PageResource"/> entry 
		///			in the configured <see cref="IStaticResourcesInfoProvider.PageResources"/> 
		///			collection by matching its <see cref="PageResource.Route"/> and 
		///			<see cref="PageResource.Query"/> properties.
		///		</para>
		///		<para>
		///			The URL must contain the page route and query parameters, if any. 
		///			<code>Example: /pages/page?p1=v1</code>
		///		</para>
		/// </param>
		/// <param name="destinationRoot">
		///		The path to the root folder where generated static page 
		///		files (and subfolders) will be placed.
		/// </param>
		/// <param name="alwaysDefaultFile">
		///		<para>
		///			Specifies whether to always create default files for pages 
		///			(true) even if a route specifies a page name, or an html file 
		///			bearing the page name (false).
		///		</para>
		///		<para>
		///			Does not affect routes that end with a trailing forward slash.
		///			A default file will always be generated for such routes.
		///		</para>
		///		<para>
		///			Whereas /page/ will always produce /page/index.html, /page will 
		///			produce /page/index.html (true) or /page.html (false).
		///		</para>
		/// </param>
		/// <param name="dontUpdateLinks">
		///		<para>
		///			Indicates, when true, that the href value of [a] and [area] 
		///			HTML tags should not be modofied to refer to the generated 
		///			static pages.
		///		</para>
		///		<para>
		///			Href values will be modified such that a value of /page is 
		///			converted to /page.html or /page/index.html depending on 
		///			<paramref name="alwaysDefaultFile"/>.
		///		</para>
		/// </param>
		/// <param name="dontOptimizeContent">
		///		<para>
		///			Specifies whether to omit optimizing the content of generated static fiels.
		///		</para>
		///		<para>
		///			By default, when this parameter is <c>false</c>, content of the generated 
		///			static file will be optimized. Specify <c>true</c> to omit the optimizations.
		///		</para>
		/// </param>
		/// <param name="httpClientName">
		///		Optional. The name of a configured HTTP client to use for fetching pages.
		/// </param>
		/// <param name="httpTimeoutSeconds">
		///		<para>
		///			The HttpClient request timeout (in seconds) while fetching page content.
		///		</para>
		///		<para>
		///			NOTE: Applies only when the HttpClient instance is created locally, or 
		///			when the timeout for the acquired instance is <see cref="Timespan.Zero"/>.
		///		</para>
		/// </param>
		/// <param name="ct">The object to monitor for cancellation requests.</param>
		/// <returns>
		///		An object representing the async operation that will return 
		///		a boolean True if the operation succeeded, or False otherwise.
		/// </returns>
		public static async Task<bool> GenerateStaticPage(
			this IHost host,
			string pageUrl,
			string destinationRoot,
			bool alwaysDefaultFile = default,
			bool dontUpdateLinks = default,
			bool dontOptimizeContent = default,
			string? httpClientName = default,
			ulong httpTimeoutSeconds = c_DefaultHttpTimeoutSeconds,
			CancellationToken ct = default)
		{
			Throw.IfNull(host);
			Throw.IfNullOrWhitespace(pageUrl);

			var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
			var logger = loggerFactory.CreateLogger(nameof(StaticGeneratorHostExtension));

			var pageUrlProvider = host.Services.GetRequiredService<IStaticResourcesInfoProvider>();

			if (!pageUrlProvider.PageResources.Any())
			{
				logger.NoPagesToProcess();
				return false;
			}

			var page = pageUrlProvider.PageResources.GetResourceForUrl(pageUrl);

			if ((page is null) || (page is not PageResource))
			{
				logger.PageNotFound(pageUrl);
				return false;
			}

			return await host.GenerateStaticPage(
				(PageResource) page,
				destinationRoot,
				alwaysDefaultFile,
				dontUpdateLinks,
				dontOptimizeContent,
				httpClientName,
				httpTimeoutSeconds,
				ct).ConfigureAwait(false);
		}

		/// <summary>
		///		Generates a static file for the specified page.
		/// </summary>
		/// <remarks>
		///		NOTE: Call this method only when the application 
		///		has 'started' and is ready to serve pages.
		/// </remarks>
		/// <param name="host">An instance of the AspNetCore app host.</param>
		/// <param name="page">The page for which to generate a static file.</param>
		/// <param name="destinationRoot">
		///		The path to the root folder where generated static page 
		///		files (and subfolders) will be placed.
		/// </param>
		/// <param name="alwaysDefaultFile">
		///		<para>
		///			Specifies whether to always create default files for pages 
		///			(true) even if a route specifies a page name, or an html file 
		///			bearing the page name (false).
		///		</para>
		///		<para>
		///			Does not affect routes that end with a trailing forward slash.
		///			A default file will always be generated for such routes.
		///		</para>
		///		<para>
		///			Whereas /page/ will always produce /page/index.html, /page will 
		///			produce /page/index.html (true) or /page.html (false).
		///		</para>
		/// </param>
		/// <param name="dontUpdateLinks">
		///		<para>
		///			Indicates, when true, that the href value of [a] and [area] 
		///			HTML tags should not be modofied to refer to the generated 
		///			static pages.
		///		</para>
		///		<para>
		///			Href values will be modified such that a value of /page is 
		///			converted to /page.html or /page/index.html depending on 
		///			<paramref name="alwaysDefaultFile"/>.
		///		</para>
		/// </param>
		/// <param name="dontOptimizeContent">
		///		<para>
		///			Specifies whether to omit optimizing the content of generated static fiels.
		///		</para>
		///		<para>
		///			By default, when this parameter is <c>false</c>, content of the generated 
		///			static file will be optimized. Specify <c>true</c> to omit the optimizations.
		///		</para>
		/// </param>
		/// <param name="httpClientName">
		///		Optional. The name of a configured HTTP client to use for fetching pages.
		/// </param>
		/// <param name="httpTimeoutSeconds">
		///		<para>
		///			The HttpClient request timeout (in seconds) while fetching page content.
		///		</para>
		///		<para>
		///			NOTE: Applies only when the HttpClient instance is created locally, or 
		///			when the timeout for the acquired instance is <see cref="Timespan.Zero"/>.
		///		</para>
		/// </param>
		/// <param name="ct">The object to monitor for cancellation requests.</param>
		/// <returns>
		///		An object representing the async operation that will return 
		///		a boolean True if the operation succeeded, or False otherwise.
		/// </returns>
		public static async Task<bool> GenerateStaticPage(
			this IHost host,
			PageResource page,
			string destinationRoot,
			bool alwaysDefaultFile = default,
			bool dontUpdateLinks = default,
			bool dontOptimizeContent = default,
			string? httpClientName = default,
			ulong httpTimeoutSeconds = c_DefaultHttpTimeoutSeconds,
			CancellationToken ct = default)
		{
			Throw.IfNull(host);
			Throw.IfNull(page);
			Throw.IfNullOrWhitespace(destinationRoot);

			var fileSystem = host.Services.GetService<IFileSystem>() ?? new FileSystem();

			Throw.DirectoryNotFoundWhen(
				() => !fileSystem.Directory.Exists(destinationRoot),
				SR.Err_InvalidDestinationRoot);

			var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
			var logger = loggerFactory.CreateLogger(nameof(StaticGeneratorHostExtension));
			var pageUrlProvider = host.Services.GetRequiredService<IStaticResourcesInfoProvider>();
			var optimizerSelector = GetOptimizerSelector(host.Services, dontOptimizeContent);
			var httpClient = GetHttpClient(host, httpClientName, httpTimeoutSeconds);

			try
			{
				await StaticGenerator.ExecuteForPage(
					page, new StaticGeneratorConfig(
						pageUrlProvider.Resources,
						destinationRoot,
						alwaysDefaultFile,
						!dontUpdateLinks,
						pageUrlProvider.DefaultFileName,
						pageUrlProvider.PageFileExtension.EnsureStartsWith('.'),
						pageUrlProvider.DefaultFileExclusions,
						!dontOptimizeContent, optimizerSelector,
						pageUrlProvider.SkipPageResources,
						pageUrlProvider.SkipCssResources,
						pageUrlProvider.SkipJsResources,
						pageUrlProvider.SkipBinResources),
					httpClient,
					fileSystem,
					loggerFactory,
					ct).ConfigureAwait(false);

				return true;
			}
			catch (OperationCanceledException) { }
			catch (Exception ex)
			{
				logger.Exception(ex);
			}

			return false;
		}


		private static string GetBaseUri(IHost host)
		{
			var hostFeatures = host.Services.GetRequiredService<IServer>().Features;
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
				SR.Err_HostNotHttpService);

			return baseUri!;
		}

		private static HttpClient GetHttpClient(IHost host, string? httpClientName, ulong httpTimeoutSeconds)
		{
			var baseUri = GetBaseUri(host);

			var httpClientFactory = host.Services.GetService<IHttpClientFactory>();

			var httpClient =
				(httpClientFactory is null)
				? new HttpClient()
				{
					BaseAddress = new Uri(baseUri),
					Timeout = TimeSpan.FromSeconds(httpTimeoutSeconds),
				}
				: string.IsNullOrWhiteSpace(httpClientName)
				? httpClientFactory.CreateClient()
				: httpClientFactory.CreateClient(httpClientName)
				;

			httpClient.BaseAddress ??= new Uri(baseUri);

			if (httpClient.Timeout == TimeSpan.Zero)
			{
				httpClient.Timeout = TimeSpan.FromSeconds(httpTimeoutSeconds);
			}

			if (!httpClient.DefaultRequestHeaders.Any(
					x =>
					x.Key.Equals(HeaderNames.UserAgent, StringComparison.OrdinalIgnoreCase) &&
					x.Value.Any(y => y.Equals(Consts.AspNetStatic, StringComparison.OrdinalIgnoreCase))))
			{
				httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, Consts.AspNetStatic);
			}

			return httpClient;
		}

		private static IOptimizerSelector? GetOptimizerSelector(IServiceProvider services, bool dontOptimizeContent)
		{
			if (dontOptimizeContent) return null;

			var result = 
				services.GetService<IOptimizerSelector>() ??
				DefaultOptimizerSelectorFactory.Create(services)
				;

			return result;
		}


		private static readonly CancellationTokenSource _appShutdown = new();
		private static readonly HttpClient _httpClient = new();
#if USE_PERIODIC_TIMER
		private static PeriodicTimer _timer = null!;
#else
		private static readonly System.Timers.Timer _timer = new();
#endif
		private const ulong c_DefaultHttpTimeoutSeconds = 100;
	}


	#region Logger Extensions...

	internal static partial class StaticPageGeneratorHostExtensionLoggerExtensions
	{
		#region 1001 - Trace

		public static void Trace(
			this ILogger logger,
			string message,
			params object[] args) =>
			logger.Imp_Trace(
				string.Format(
					CultureInfo.InvariantCulture,
					message, args));

		[LoggerMessage(EventId = 1001, EventName = "Trace", Level = LogLevel.Trace,
			Message = "StaticPageGeneratorHost: Trace > Message = {Message}")]
		private static partial void Imp_Trace(
			this ILogger logger,
			string message);

		#endregion
		#region 1002 - Debug

		public static void Debug(
			this ILogger logger,
			string message,
			params object[] args) =>
			logger.Imp_Debug(
				string.Format(
					CultureInfo.InvariantCulture,
					message, args));

		[LoggerMessage(EventId = 1002, EventName = "Debug", Level = LogLevel.Debug,
			Message = "StaticPageGeneratorHost: Debug > Message = {Message}")]
		private static partial void Imp_Debug(
			this ILogger logger,
			string message);

		#endregion
		#region 1003 - Exception

		public static void Exception(
			this ILogger logger,
			Exception ex) =>
			logger.Imp_Exception(
				ex.Message,
				ex);

		[LoggerMessage(EventId = 1003, EventName = "Exception", Level = LogLevel.Critical,
			Message = "StaticPageGeneratorHost: Exception > Error = {Error}")]
		private static partial void Imp_Exception(
			this ILogger logger,
			string error,
			Exception ex);

		#endregion

		#region 1010 - NoPagesToProcess

		public static void NoPagesToProcess(
			this ILogger logger) =>
			logger.Imp_NoPagesToProcess();

		[LoggerMessage(EventId = 1010, EventName = "NoPagesToProcess", Level = LogLevel.Information,
			Message = "StaticPageGeneratorHost: No pages to process. Exiting...")]
		private static partial void Imp_NoPagesToProcess(
			this ILogger logger);

		#endregion
		#region 1012 - PageNotFound

		public static void PageNotFound(
			this ILogger logger,
			string pageUrl) =>
			logger.Imp_PageNotFound(
				pageUrl);

		[LoggerMessage(EventId = 1012, EventName = "PageNotFound", Level = LogLevel.Information,
			Message = "StaticPageGeneratorHost: Could not find page entry for specified url > PageUrl = {PageUrl}")]
		private static partial void Imp_PageNotFound(
			this ILogger logger,
			string pageUrl);

		#endregion

		#region 1020 - Exiting

		public static void Exiting(
			this ILogger logger) =>
			logger.Imp_Exiting();

		[LoggerMessage(EventId = 1020, EventName = "Exiting", Level = LogLevel.Information,
			Message = "StaticPageGeneratorHost: Completed generating static pages. Exiting...")]
		private static partial void Imp_Exiting(
			this ILogger logger);

		#endregion

		#region 1030 - RegenerationConfig

		public static void RegenerationConfig(
			this ILogger logger,
			TimeSpan? interval) =>
			logger.Imp_RegenerationConfig(
				interval);

		[LoggerMessage(EventId = 1030, EventName = "RegenerationConfig", Level = LogLevel.Information,
			Message = "StaticPageGeneratorHost: Periodic Regeneration > Interval = {Interval}")]
		private static partial void Imp_RegenerationConfig(
			this ILogger logger,
			TimeSpan? interval);

		#endregion
	}

	#endregion
}
