/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023 Zareh DerGevorkian

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License. 
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
--------------------------------------------------------------------------------------------------------------------------------*/

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
		public static void GenerateStaticPages(
			this IHost host,
			string destinationRoot,
			string[] commandLineArgs,
			bool alwaysDefautFile = default,
			bool dontUpdateLinks = default,
			bool dontOptimizeContent = default) =>
			host.GenerateStaticPages(
				destinationRoot,
				commandLineArgs.HasExitAfterStaticGenerationParameter(),
				alwaysDefautFile, dontUpdateLinks, dontOptimizeContent);

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
		public static void GenerateStaticPages(
			this IHost host,
			string destinationRoot,
			bool exitWhenDone = default,
			bool alwaysDefautFile = default,
			bool dontUpdateLinks = default,
			bool dontOptimizeContent = default)
		{
			if (host is null)
			{
				throw new ArgumentNullException(nameof(host));
			}
			if (string.IsNullOrWhiteSpace(destinationRoot))
			{
				throw new ArgumentNullException(nameof(destinationRoot));
			}
			if (!Directory.Exists(destinationRoot))
			{
				throw new InvalidOperationException(
					Properties.Resources.Err_InvalidDestinationRoot);
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

			var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

			lifetime.ApplicationStarted.Register(
				async () =>
				{
					try
					{
						var hostFeatures = host.Services.GetRequiredService<IServer>().Features;
						var serverAddresses = hostFeatures.Get<IServerAddressesFeature>() ??
							throw new InvalidOperationException($"Feature '{typeof(IServerAddressesFeature)}' is not present.");
						var hostUrls = serverAddresses.Addresses;

						var baseUri =
							hostUrls.FirstOrDefault(x => x.StartsWith(Uri.UriSchemeHttps)) ??
							hostUrls.FirstOrDefault(x => x.StartsWith(Uri.UriSchemeHttp)) ??
							throw new InvalidOperationException(Properties.Resources.Err_HostNotHttpService);

						using var httpClient =
							new HttpClient()
							{
								BaseAddress = new Uri(baseUri),
								Timeout = TimeSpan.FromSeconds(90)
							};

						httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, nameof(AspNetStatic));

						await StaticPageGenerator.Execute(new(
							httpClient,
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
							jsMinifier),
							loggerFactory);
					}
					catch (Exception ex)
					{
						logger.Exception(ex);
					}

					if (exitWhenDone)
					{
						logger.Exiting();
						await Task.Delay(500);
						await host.StopAsync();
					}
				});
		}

		private const string STATIC_ONLY = "static-only";

		public static bool HasExitAfterStaticGenerationParameter(this string[] args) => 
			(args is not null) && args.Any(a => a.Equals(
				STATIC_ONLY, StringComparison.InvariantCultureIgnoreCase));
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
	}

	#endregion
}
