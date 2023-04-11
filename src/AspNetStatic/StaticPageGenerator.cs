/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023 Zareh DerGevorkian

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License. 
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
--------------------------------------------------------------------------------------------------------------------------------*/

using System.Text;
using Microsoft.Extensions.Logging;
using WebMarkupMin.Core;

namespace AspNetStatic
{
	public class StaticPageGenerator
	{
		public static async Task Execute(
			StaticPageGeneratorConfig config,
			ILoggerFactory? loggerFactory = default,
			CancellationToken ct = default)
		{
			if (ct.IsCancellationRequested) return;

			if (config is null) throw new ArgumentNullException(nameof(config));

			var logger = loggerFactory?.CreateLogger<StaticPageGenerator>();

			IMarkupMinifier htmlMinifier = null!;
			if (config.OptimizePageContent)
			{
				htmlMinifier =
					new HtmlMinifier(
						config.HtmlMinifierSettings,
						config.CssMinifier,
						config.JsMinifier);
			}

			logger?.Configuration(config);

			logger?.GeneratingStaticPages();

			foreach (var page in config.Pages)
			{
				if (ct.IsCancellationRequested) break;

				logger?.ProcessingPage(page);

				var pagePath =
					RouteToPathname.GetPathname(
						page, config.DestinationRoot,
						config.AlwaysCreateDefaultFile,
						config.IndexFileName, config.PageFileExtension,
						config.DefaultFileExclusions.ToArray());

				var pagePathShortName = pagePath.Replace(config.DestinationRoot, string.Empty);

				logger?.Debug("PagePath = {0}", pagePathShortName);

				RemoveIfExists(pagePath);

				if (!EnsureFolderExists(pagePath))
				{
					logger?.FailedToEnsureFolder(pagePath);
					continue;
				}

				var requestUri = page.Url;

				logger?.FetchingPageContent(requestUri);

				var pageContent = string.Empty;

				try
				{
					pageContent = await config.HttpClient.GetStringAsync(requestUri, ct);
				}
				catch (Exception ex)
				{
					logger?.Exception(ex);
				}

				if (!string.IsNullOrWhiteSpace(pageContent))
				{
					if (config.UpdateLinks)
					{
						logger?.UpdatingHrefValues(pagePathShortName);

						pageContent =
							pageContent.FixupHrefValues(
								config.Pages,
								config.IndexFileName, config.PageFileExtension,
								config.AlwaysCreateDefaultFile);
					}

					if (config.OptimizePageContent)
					{
						logger?.OptimizingHtmlContent(requestUri, pagePathShortName);

						var result = htmlMinifier.Minify(pageContent, Encoding.UTF8);

						if (!result.Errors.Any())
						{
							pageContent = result.MinifiedContent;
						}
						else
						{
							logger?.OptimizationErrors(requestUri, pagePathShortName, result.Errors);
						}
						if (result.Warnings.Any())
						{
							logger?.OptimizationErrors(requestUri, pagePathShortName, result.Warnings);
						}
					}

					logger?.WritingPageFile(requestUri, pagePathShortName, pageContent.Length);

					await File.WriteAllTextAsync(pagePath, pageContent, Encoding.UTF8, ct); //
				}
				else
				{
					logger?.NoContentForPage(requestUri);
				}
			}
		}


		private static void RemoveIfExists(string pageDiskPath)
		{
			if (string.IsNullOrWhiteSpace(pageDiskPath))
			{
				throw new ArgumentNullException(nameof(pageDiskPath));
			}

			if (File.Exists(pageDiskPath))
			{
				File.Delete(pageDiskPath);
			}
		}

		private static bool EnsureFolderExists(string pageDiskPath)
		{
			if (string.IsNullOrWhiteSpace(pageDiskPath))
			{
				throw new ArgumentNullException(nameof(pageDiskPath));
			}

			var folder = Directory.GetParent(pageDiskPath);

			if (folder is null) return false;

			if (!folder.Exists)
			{
				folder?.Create();
			}

			return true;
		}
	}


	#region Logger Extensions...

	internal static partial class StaticPageGeneratorLoggerExtensions
	{
		#region 9001 - Configuration

		public static void Configuration(
			this ILogger<StaticPageGenerator> logger,
			StaticPageGeneratorConfig config) =>
			logger.Imp_Configuration(
				config.Pages.Count,
				config.HttpClient.BaseAddress?.ToString() ?? "Unknown Base Address",
				config.DestinationRoot,
				config.AlwaysCreateDefaultFile,
				config.UpdateLinks,
				config.DefaultFileName,
				config.PageFileExtension,
				config.DefaultFileExclusions);

		[LoggerMessage(EventId = 9001, EventName = "Configuration", Level = LogLevel.Information,
			Message = "StaticPageGenerator: Configuration > PageCount = {PageCount}, BaseUri = {BaseUri}, DestinationRoot = {DestinationRoot}, AlwaysDefaultFile = {AlwaysDefaultFile}, UpdateLinks = {UpdateLinks}, DefaultFileName = {DefaultFileName}, PageFileExtension = {PageFileExtension}, DefaultFileExclusions = {DefaultFileExclusions}")]
		private static partial void Imp_Configuration(
			this ILogger<StaticPageGenerator> logger,
			int pageCount,
			string baseUri,
			string destinationRoot,
			bool alwaysDefaultFile,
			bool updateLinks,
			string defaultFileName,
			string pageFileExtension,
			IEnumerable<string> defaultFileExclusions);

		#endregion

		#region 1001 - Trace

		public static void Trace(
			this ILogger<StaticPageGenerator> logger,
			string message,
			params object[] args) =>
			logger.Imp_Trace(
				string.Format(
					CultureInfo.InvariantCulture,
					message, args));

		[LoggerMessage(EventId = 1001, EventName = "Trace", Level = LogLevel.Trace,
			Message = "StaticPageGenerator: Trace > Message = {Message}")]
		private static partial void Imp_Trace(
			this ILogger<StaticPageGenerator> logger,
			string message);

		#endregion
		#region 1002 - Debug

		public static void Debug(
			this ILogger<StaticPageGenerator> logger,
			string message,
			params object[] args) =>
			logger.Imp_Debug(
				string.Format(
					CultureInfo.InvariantCulture,
					message, args));

		[LoggerMessage(EventId = 1002, EventName = "Debug", Level = LogLevel.Debug,
			Message = "StaticPageGenerator: Debug > Message = {Message}")]
		private static partial void Imp_Debug(
			this ILogger<StaticPageGenerator> logger,
			string message);

		#endregion
		#region 1003 - Exception

		public static void Exception(
			this ILogger<StaticPageGenerator> logger,
			Exception ex) =>
			logger.Imp_Exception(
				ex.Message,
				ex);

		[LoggerMessage(EventId = 1003, EventName = "Exception", Level = LogLevel.Critical,
			Message = "StaticPageGenerator: Exception > Error = {Error}")]
		private static partial void Imp_Exception(
			this ILogger<StaticPageGenerator> logger,
			string error,
			Exception ex);

		#endregion

		#region 1010 - GeneratingStaticPages

		public static void GeneratingStaticPages(
			this ILogger<StaticPageGenerator> logger) =>
			logger.Imp_GeneratingStaticPages();

		[LoggerMessage(EventId = 1010, EventName = "GeneratingStaticPages", Level = LogLevel.Information,
			Message = "StaticPageGenerator: Generating static pages...")]
		private static partial void Imp_GeneratingStaticPages(
			this ILogger<StaticPageGenerator> logger);

		#endregion

		#region 1020 - NoPagesToProcess

		public static void NoPagesToProcess(
			this ILogger<StaticPageGenerator> logger) =>
			logger.Imp_NoPagesToProcess();

		[LoggerMessage(EventId = 1020, EventName = "NoPagesToProcess", Level = LogLevel.Information,
			Message = "StaticPageGenerator: No pages to process. Exiting...")]
		private static partial void Imp_NoPagesToProcess(
			this ILogger<StaticPageGenerator> logger);

		#endregion

		#region 1030 - ProcessingPage

		public static void ProcessingPage(
			this ILogger<StaticPageGenerator> logger,
			PageInfo page) =>
			logger.Imp_ProcessingPage(
				page.Route,
				page.QueryString);

		[LoggerMessage(EventId = 1030, EventName = "ProcessingPage", Level = LogLevel.Information,
			Message = "StaticPageGenerator: Processing page > Route = {Route}, Query = {Query}")]
		private static partial void Imp_ProcessingPage(
			this ILogger<StaticPageGenerator> logger,
			string route,
			string? query);

		#endregion

		#region 1040 - FailedToEnsureFolder

		public static void FailedToEnsureFolder(
			this ILogger<StaticPageGenerator> logger,
			string pagePath) =>
			logger.Imp_FailedToEnsureFolder(
				pagePath);

		[LoggerMessage(EventId = 1040, EventName = "FailedToEnsureFolder", Level = LogLevel.Error,
			Message = "StaticPageGenerator: Failed to ensure folder exists for page > PagePath = {PagePath}")]
		private static partial void Imp_FailedToEnsureFolder(
			this ILogger<StaticPageGenerator> logger,
			string pagePath);

		#endregion

		#region 1050 - FetchingPageContent

		public static void FetchingPageContent(
			this ILogger<StaticPageGenerator> logger,
			string pageUrl) =>
			logger.Imp_FetchingPageContent(
				pageUrl);

		[LoggerMessage(EventId = 1050, EventName = "FetchingPageContent", Level = LogLevel.Trace,
			Message = "StaticPageGenerator: Fetching page content > PageUrl = {PageUrl}")]
		private static partial void Imp_FetchingPageContent(
			this ILogger<StaticPageGenerator> logger,
			string pageUrl);

		#endregion

		#region 1060 - UpdatingHrefValues

		public static void UpdatingHrefValues(
			this ILogger<StaticPageGenerator> logger,
			string pageFile) =>
			logger.Imp_UpdatingHrefValues(
				pageFile);

		[LoggerMessage(EventId = 1060, EventName = "UpdatingHrefValues", Level = LogLevel.Trace,
			Message = "StaticPageGenerator: Updating href values in page content > PageFile = {PageFile}")]
		private static partial void Imp_UpdatingHrefValues(
			this ILogger<StaticPageGenerator> logger,
			string pageFile);

		#endregion

		#region 1070 - NoContentForPage

		public static void NoContentForPage(
			this ILogger<StaticPageGenerator> logger,
			string pageUrl) =>
			logger.Imp_NoContentForPage(
				pageUrl);

		[LoggerMessage(EventId = 1070, EventName = "NoContentForPage", Level = LogLevel.Warning,
			Message = "StaticPageGenerator: No content for page > PageUrl = {PageUrl}")]
		private static partial void Imp_NoContentForPage(
			this ILogger<StaticPageGenerator> logger,
			string pageUrl);

		#endregion

		#region 1080 - OptimizingHtmlContent

		public static void OptimizingHtmlContent(
			this ILogger<StaticPageGenerator> logger,
			string pageUrl,
			string pagePath) =>
			logger.Imp_OptimizingHtmlContent(
				pageUrl,
				pagePath);

		[LoggerMessage(EventId = 1080, EventName = "OptimizingHtmlContent", Level = LogLevel.Information,
			Message = "StaticPageGenerator: Optimizing page content > PageUrl = {PageUrl}, PagePath = {PagePath}")]
		private static partial void Imp_OptimizingHtmlContent(
			this ILogger<StaticPageGenerator> logger,
			string pageUrl,
			string pagePath);

		#endregion
		#region 1081 - OptimizationErrors

		public static void OptimizationErrors(
			this ILogger<StaticPageGenerator> logger,
			string pageUrl,
			string pagePath,
			IList<MinificationErrorInfo> errors) =>
			logger.Imp_OptimizationErrors(
				pageUrl,
				pagePath,
				errors);

		[LoggerMessage(EventId = 1081, EventName = "OptimizationErrors", Level = LogLevel.Error,
			Message = "StaticPageGenerator: Content optimizing Errors > PageUrl = {PageUrl}, PagePath = {PagePath}, Errors = {Errors}")]
		private static partial void Imp_OptimizationErrors(
			this ILogger<StaticPageGenerator> logger,
			string pageUrl,
			string pagePath,
			IList<MinificationErrorInfo> errors);

		#endregion
		#region 1082 - OptimizationWarnings

		public static void OptimizationWarnings(
			this ILogger<StaticPageGenerator> logger,
			string pageUrl,
			string pagePath,
			IList<MinificationErrorInfo> warnings) =>
			logger.Imp_OptimizationWarnings(
				pageUrl,
				pagePath,
				warnings);

		[LoggerMessage(EventId = 1082, EventName = "OptimizationWarnings", Level = LogLevel.Warning,
			Message = "StaticPageGenerator: Content optimizing warnings > PageUrl = {PageUrl}, PagePath = {PagePath}, Warnings = {Warnings}")]
		private static partial void Imp_OptimizationWarnings(
			this ILogger<StaticPageGenerator> logger,
			string pageUrl,
			string pagePath,
			IList<MinificationErrorInfo> warnings);

		#endregion

		#region 1090 - WritingPageFile

		public static void WritingPageFile(
			this ILogger<StaticPageGenerator> logger,
			string pageUrl,
			string pagePath,
			int contentSize) =>
			logger.Imp_WritingPageFile(
				pageUrl,
				pagePath,
				contentSize);

		[LoggerMessage(EventId = 1090, EventName = "WritingPageFile", Level = LogLevel.Information,
			Message = "StaticPageGenerator: Writing page content to file > PageUrl = {PageUrl}, PagePath = {PagePath}, ContentSize = {ContentSize}")]
		private static partial void Imp_WritingPageFile(
			this ILogger<StaticPageGenerator> logger,
			string pageUrl,
			string pagePath,
			int contentSize);

		#endregion
	}

	#endregion
}
