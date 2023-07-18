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
using Microsoft.Extensions.Logging;
using WebMarkupMin.Core;

namespace AspNetStatic
{
	internal class StaticPageGenerator
	{
		public static async Task Execute(
			StaticPageGeneratorConfig config,
			HttpClient httpClient,
			IFileSystem fileSystem,
			ILoggerFactory? loggerFactory = default,
			CancellationToken ct = default)
		{
			if (ct.IsCancellationRequested) return;

			Throw.IfNull(config);
			Throw.IfNull(httpClient);
			Throw.IfNull(fileSystem);

			var logger = loggerFactory?.CreateLogger<StaticPageGenerator>();

			logger?.GeneratingStaticPages();
			logger?.Configuration(config, httpClient);

			var optimizeOutput = config.OptimizePageContent;
			var optimizerSelector = config.OptimizerSelector;

			if (optimizeOutput && (optimizerSelector is null))
			{
				Throw.InvalidOp(Properties.Resources.Err_OptimizeWithoutChooser);
			}

			var destRoot = config.DestinationRoot;
			var alwaysDefaultFile = config.AlwaysCreateDefaultFile;
			var indexFile = config.IndexFileName;
			var fileExtension = config.PageFileExtension;
			var exclusionsArray = config.DefaultFileExclusions.ToArray();

			foreach (var page in config.Pages)
			{
				if (ct.IsCancellationRequested) break;

				logger?.ProcessingPage(page);

				var outFilePathname = page.GetOutFilePathname(
					destRoot, alwaysDefaultFile,
					indexFile, fileExtension,
					exclusionsArray);

				var outFileShortName = outFilePathname.Replace(config.DestinationRoot, string.Empty);

				logger?.Debug("PagePath = {0}", outFileShortName);

				RemoveIfExists(fileSystem, outFilePathname);

				if (!EnsureFolderExists(fileSystem, outFilePathname))
				{
					logger?.FailedToEnsureFolder(outFilePathname);
					continue;
				}

				var requestUri = page.Url;

				logger?.FetchingPageContent(requestUri);

				var pageContent = string.Empty;

				try
				{
					pageContent = await httpClient.GetStringAsync(requestUri, ct);
				}
				catch (Exception ex)
				{
					logger?.Exception(ex);
				}

				if (string.IsNullOrWhiteSpace(pageContent))
				{
					logger?.NoContentForPage(requestUri);
					continue;
				}

				if (config.UpdateLinks)
				{
					logger?.UpdatingHrefValues(outFileShortName);

					pageContent =
						pageContent.FixupHrefValues(
							config.Pages,
							config.IndexFileName, config.PageFileExtension,
							config.AlwaysCreateDefaultFile);
				}

				if (optimizeOutput && !page.SkipOptimization)
				{
					logger?.OptimizingContent(requestUri, outFileShortName);

					var optimizer = optimizerSelector!.SelectFor(page, outFilePathname);

					logger?.SelectedOptimizer(requestUri, outFileShortName, optimizer);

					var result = optimizer.Minify(pageContent, page.OutputEncoding.ToSystemEncoding());

					if (!result.Errors.Any())
					{
						pageContent = result.MinifiedContent;
					}
					else
					{
						logger?.OptimizationErrors(requestUri, outFileShortName, result.Errors);
					}
					if (result.Warnings.Any())
					{
						logger?.OptimizationErrors(requestUri, outFileShortName, result.Warnings);
					}
				}

				logger?.WritingPageFile(requestUri, outFileShortName, pageContent.Length, page.OutputEncoding);

				await fileSystem.File.WriteAllTextAsync(outFilePathname, pageContent, page.OutputEncoding.ToSystemEncoding(), ct);
			}
		}

		public static async Task ExecuteForPage(
			PageInfo page,
			StaticPageGeneratorConfig config,
			HttpClient httpClient,
			IFileSystem fileSystem,
			ILoggerFactory? loggerFactory = default,
			CancellationToken ct = default)
		{
			if (ct.IsCancellationRequested) return;

			Throw.IfNull(page);
			Throw.IfNull(config);
			Throw.IfNull(httpClient);
			Throw.IfNull(fileSystem);

			var logger = loggerFactory?.CreateLogger<StaticPageGenerator>();

			logger?.GeneratingStaticPages();
			logger?.ConfigurationSinglePage(page, config, httpClient);

			var optimizeOutput = config.OptimizePageContent;
			var optimizerSelector = config.OptimizerSelector;

			if (optimizeOutput && (optimizerSelector is null))
			{
				Throw.InvalidOp(Properties.Resources.Err_OptimizeWithoutChooser);
			}

			var destRoot = config.DestinationRoot;
			var alwaysDefaultFile = config.AlwaysCreateDefaultFile;
			var indexFile = config.IndexFileName;
			var fileExtension = config.PageFileExtension;
			var exclusionsArray = config.DefaultFileExclusions.ToArray();

			if (ct.IsCancellationRequested) return;

			logger?.ProcessingPage(page);

			var outFilePathname = page.GetOutFilePathname(
				destRoot, alwaysDefaultFile,
				indexFile, fileExtension,
				exclusionsArray);

			var outFileShortName = outFilePathname.Replace(config.DestinationRoot, string.Empty);

			logger?.Debug("PagePath = {0}", outFileShortName);

			RemoveIfExists(fileSystem, outFilePathname);

			if (!EnsureFolderExists(fileSystem, outFilePathname))
			{
				logger?.FailedToEnsureFolder(outFilePathname);
				return;
			}

			var requestUri = page.Url;

			logger?.FetchingPageContent(requestUri);

			var pageContent = string.Empty;

			try
			{
				pageContent = await httpClient.GetStringAsync(requestUri, ct);
			}
			catch (Exception ex)
			{
				logger?.Exception(ex);
			}

			if (string.IsNullOrWhiteSpace(pageContent))
			{
				logger?.NoContentForPage(requestUri);
				return;
			}

			if (config.UpdateLinks)
			{
				logger?.UpdatingHrefValues(outFileShortName);

				pageContent =
					pageContent.FixupHrefValues(
						config.Pages,
						config.IndexFileName, config.PageFileExtension,
						config.AlwaysCreateDefaultFile);
			}

			if (optimizeOutput && !page.SkipOptimization)
			{
				logger?.OptimizingContent(requestUri, outFileShortName);

				var optimizer = optimizerSelector!.SelectFor(page, outFilePathname);

				logger?.SelectedOptimizer(requestUri, outFileShortName, optimizer);

				var result = optimizer.Minify(pageContent, page.OutputEncoding.ToSystemEncoding());

				if (!result.Errors.Any())
				{
					pageContent = result.MinifiedContent;
				}
				else
				{
					logger?.OptimizationErrors(requestUri, outFileShortName, result.Errors);
				}
				if (result.Warnings.Any())
				{
					logger?.OptimizationErrors(requestUri, outFileShortName, result.Warnings);
				}
			}

			logger?.WritingPageFile(requestUri, outFileShortName, pageContent.Length, page.OutputEncoding);

			await fileSystem.File.WriteAllTextAsync(outFilePathname, pageContent, page.OutputEncoding.ToSystemEncoding(), ct);
		}

		private static void RemoveIfExists(IFileSystem fs, string pageDiskPath)
		{
			Throw.IfNull(fs, nameof(fs));
			Throw.IfNullOrWhiteSpace(
				pageDiskPath, nameof(pageDiskPath),
				Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace);

			if (fs.File.Exists(pageDiskPath))
			{
				fs.File.Delete(pageDiskPath);
			}
		}

		private static bool EnsureFolderExists(IFileSystem fs, string pageDiskPath)
		{
			Throw.IfNull(fs, nameof(fs));
			Throw.IfNullOrWhiteSpace(
				pageDiskPath, nameof(pageDiskPath),
				Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace);

			var folder = fs.Directory.GetParent(pageDiskPath);

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
			StaticPageGeneratorConfig config,
			HttpClient httpClient) =>
			logger.Imp_Configuration(
				config.Pages.Count,
				httpClient.BaseAddress?.ToString() ?? "Unknown Base Address",
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
		#region 9002 - ConfigurationSinglePage

		public static void ConfigurationSinglePage(
			this ILogger<StaticPageGenerator> logger,
			PageInfo page,
			StaticPageGeneratorConfig config,
			HttpClient httpClient) =>
			logger.Imp_ConfigurationSinglePage(
				page.Url,
				config.Pages.Count,
				httpClient.BaseAddress?.ToString() ?? "Unknown Base Address",
				config.DestinationRoot,
				config.AlwaysCreateDefaultFile,
				config.UpdateLinks,
				config.DefaultFileName,
				config.PageFileExtension,
				config.DefaultFileExclusions);

		[LoggerMessage(EventId = 9002, EventName = "ConfigurationSinglePage", Level = LogLevel.Information,
			Message = "StaticPageGenerator: ConfigurationSinglePage > PageUrl = {PageUrl}, PageCount = {PageCount}, BaseUri = {BaseUri}, DestinationRoot = {DestinationRoot}, AlwaysDefaultFile = {AlwaysDefaultFile}, UpdateLinks = {UpdateLinks}, DefaultFileName = {DefaultFileName}, PageFileExtension = {PageFileExtension}, DefaultFileExclusions = {DefaultFileExclusions}")]
		private static partial void Imp_ConfigurationSinglePage(
			this ILogger<StaticPageGenerator> logger,
			string pageUrl,
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
				page.Query);

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

		#region 1080 - OptimizingContent

		public static void OptimizingContent(
			this ILogger<StaticPageGenerator> logger,
			string pageUrl,
			string pagePath) =>
			logger.Imp_OptimizingContent(
				pageUrl,
				pagePath);

		[LoggerMessage(EventId = 1080, EventName = "OptimizingContent", Level = LogLevel.Information,
			Message = "StaticPageGenerator: Optimizing page content > PageUrl = {PageUrl}, PagePath = {PagePath}")]
		private static partial void Imp_OptimizingContent(
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
		#region 1083 - SelectedOptimizer

		public static void SelectedOptimizer(
			this ILogger<StaticPageGenerator> logger,
			string pageUrl,
			string pagePath,
			IMarkupMinifier optimizer) =>
			logger.Imp_SelectedOptimizer(
				pageUrl,
				pagePath,
				optimizer.GetType().Name);

		[LoggerMessage(EventId = 1083, EventName = "SelectedOptimizer", Level = LogLevel.Debug,
			Message = "StaticPageGenerator: Selected optimizer > PageUrl = {PageUrl}, PagePath = {PagePath}, Optimizer = {Optimizer}")]
		private static partial void Imp_SelectedOptimizer(
			this ILogger<StaticPageGenerator> logger,
			string pageUrl,
			string pagePath,
			string optimizer);

		#endregion

		#region 1090 - WritingPageFile

		public static void WritingPageFile(
			this ILogger<StaticPageGenerator> logger,
			string pageUrl,
			string pagePath,
			int contentSize,
			EncodingType encoding) =>
			logger.Imp_WritingPageFile(
				pageUrl,
				pagePath,
				contentSize,
				encoding);

		[LoggerMessage(EventId = 1090, EventName = "WritingPageFile", Level = LogLevel.Information,
			Message = "StaticPageGenerator: Writing page content to file > PageUrl = {PageUrl}, PagePath = {PagePath}, ContentSize = {ContentSize}, Encoding = {Encoding}")]
		private static partial void Imp_WritingPageFile(
			this ILogger<StaticPageGenerator> logger,
			string pageUrl,
			string pagePath,
			int contentSize,
			EncodingType encoding);

		#endregion
	}

	#endregion
}
