/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023 Zareh DerGevorkian

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License. 
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
--------------------------------------------------------------------------------------------------------------------------------*/

#define USE_PERIODIC_TIMER

using System.IO.Abstractions;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using WebMarkupMin.Core;

namespace AspNetStatic
{
	public static class StaticPageGeneratorHostExtension
	{
		/// <summary>
		///		Generates static pages for the configured pages.
		/// </summary>
		/// <param name="host"></param>
		/// <param name="commandLineArgs">
		///		The commandline arguments passed to your web app. The args 
		///		are examined for the "static-only" parameter.
		/// </param>
		/// <param name="destinationRoot"></param>
		/// <param name="alwaysDefautFile"></param>
		/// <param name="dontUpdateLinks"></param>
		/// <param name="dontOptimizeContent"></param>
		/// <param name="regenerationInterval"></param>
		public static void GenerateStaticPages(
			this IHost host,
			string destinationRoot,
			string[] commandLineArgs,
			bool alwaysDefautFile = default,
			bool dontUpdateLinks = default,
			bool dontOptimizeContent = default,
			TimeSpan? regenerationInterval = default) =>
			host.GenerateStaticPages(
				destinationRoot,
				commandLineArgs.HasExitAfterStaticGenerationParameter(),
				alwaysDefautFile, dontUpdateLinks, dontOptimizeContent,
				regenerationInterval);

		/// <summary>
		///		Generates static pages for the configured pages.
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
		/// <param name="alwaysDefautFile">
		///		<para>
		///			Specifies whether to always create default files for pages 
		///			(true) even if a route specifies a page name, or an html file 
		///			bearing the page name (false).
		///		</para>
		///		<para>
		///			Does not affct routes that end with a trailing forward slash.
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
		///			<paramref name="alwaysDefautFile"/>.
		///		</para>
		/// </param>
		/// <param name="dontOptimizeContent">
		///		<para>
		///			Specifies whether to NOT optimize the content of generated static fiels.
		///		</para>
		///		<para>
		///			By default, when this parameter is <c>false</c>, content of the generated 
		///			static file will be minified. Specify <c>true</c> to omit the optimizations.
		///		</para>
		/// </param>
		/// <param name="regenerationInterval">
		///		Specifies whther the periodic regeneration is enabled (non-null value),
		///		and the interval between regeneration events.
		/// </param>
		public static void GenerateStaticPages(
			this IHost host,
			string destinationRoot,
			bool exitWhenDone = default,
			bool alwaysDefautFile = default,
			bool dontUpdateLinks = default,
			bool dontOptimizeContent = default,
			TimeSpan? regenerationInterval = default)
		{
			Throw.IfNull(host, nameof(host));
			Throw.IfNullOrWhiteSpace(destinationRoot, nameof(destinationRoot), Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace);

			var fileSystem = host.Services.GetService<IFileSystem>() ?? new FileSystem();

			if (!fileSystem.Directory.Exists(destinationRoot))
			{
				Throw.InvalidOp(Properties.Resources.Err_InvalidDestinationRoot);
			}

			var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
			var logger = loggerFactory.CreateLogger(nameof(StaticPageGeneratorHostExtension));
			var pageUrlProvider = host.Services.GetRequiredService<IStaticPagesInfoProvider>();

			if (!pageUrlProvider.Pages.Any())
			{
				logger.NoPagesToProcess();
				return;
			}

			var htmlMinifierSettings = host.Services.GetService<HtmlMinificationSettings>();
			var cssMinifier = host.Services.GetService<ICssMinifier>();
			var jsMinifier = host.Services.GetService<IJsMinifier>();
			var xmlMinifierSettings = host.Services.GetService<XmlMinificationSettings>();

			var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

			lifetime.ApplicationStopping.Register(() => _appShutdown.Cancel());

			lifetime.ApplicationStarted.Register(
				async () =>
				{
					try
					{
						var hostFeatures = host.Services.GetRequiredService<IServer>().Features;
						var serverAddresses = hostFeatures.Get<IServerAddressesFeature>();
						if (serverAddresses is null) Throw.InvalidOp($"Feature '{typeof(IServerAddressesFeature)}' is not present.");
						var hostUrls = serverAddresses.Addresses;

						var baseUri =
							hostUrls.FirstOrDefault(x => x.StartsWith(Uri.UriSchemeHttps)) ??
							hostUrls.FirstOrDefault(x => x.StartsWith(Uri.UriSchemeHttp));
						if (baseUri is null) Throw.InvalidOp(Properties.Resources.Err_HostNotHttpService);

						_httpClient.BaseAddress = new Uri(baseUri);
						_httpClient.Timeout = TimeSpan.FromSeconds(90);
						_httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, Consts.AspNetStatic);

						var generatorConfig =
							new StaticPageGeneratorConfig(
								pageUrlProvider.Pages,
								destinationRoot,
								alwaysDefautFile,
								!dontUpdateLinks,
								pageUrlProvider.DefaultFileName,
								pageUrlProvider.PageFileExtension.EnsureStartsWith('.'),
								pageUrlProvider.DefaultFileExclusions,
								dontOptimizeContent,
								htmlMinifierSettings,
								cssMinifier,
								jsMinifier,
								xmlMinifierSettings);

						logger.RegenerationConfig(regenerationInterval);
						var doPeriodicRefresh = regenerationInterval is not null;

#if USE_PERIODIC_TIMER
						if (!_appShutdown.IsCancellationRequested)
						{
							if (doPeriodicRefresh) _timer = new(regenerationInterval!.Value);
							do
							{
								await StaticPageGenerator.Execute(
									generatorConfig,
									_httpClient, fileSystem,
									loggerFactory, _appShutdown.Token);
							}
							while (doPeriodicRefresh && await _timer.WaitForNextTickAsync(_appShutdown.Token));
						}

						if (exitWhenDone && !doPeriodicRefresh && !_appShutdown.IsCancellationRequested)
						{
							logger.Exiting();
							await Task.Delay(500);
							await host.StopAsync();
						}
#else
						await StaticPageGenerator.Execute(
							generatorConfig,
							_httpClient, fileSystem,
							loggerFactory, _appShutdown.Token);

						if (doPeriodicRefresh && !_appShutdown.IsCancellationRequested)
						{
							_timer.Interval = regenerationInterval!.Value.TotalMilliseconds;
							_timer.Elapsed +=
								async (s, e) =>
								{
									await StaticPageGenerator.Execute(
										generatorConfig,
										_httpClient, fileSystem,
										loggerFactory, _appShutdown.Token);

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
							await Task.Delay(500);
							await host.StopAsync();
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

		public static bool HasExitAfterStaticGenerationParameter(this string[] args) =>
			(args is not null) && args.Any(a => a.HasSameText(STATIC_ONLY));

		private static readonly CancellationTokenSource _appShutdown = new();
		private static readonly HttpClient _httpClient = new();
#if USE_PERIODIC_TIMER
		private static PeriodicTimer _timer = null!;
#else
		private static readonly System.Timers.Timer _timer = new();
#endif

		private const string STATIC_ONLY = "static-only";
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
