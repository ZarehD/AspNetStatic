/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023-2025 Zareh DerGevorkian

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License. 
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
--------------------------------------------------------------------------------------------------------------------------------*/

using System.Diagnostics.Contracts;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using AspNetStatic.Optimizer;
using Microsoft.Extensions.Logging;
using WebMarkupMin.Core;

namespace AspNetStatic;

internal sealed class StaticGenerator
{
	public static async Task Execute(
		StaticGeneratorConfig config,
		HttpClient httpClient,
		IFileSystem fileSystem,
		ILoggerFactory? loggerFactory = default,
		CancellationToken ct = default)
	{
		if (ct.IsCancellationRequested) return;

		Throw.IfNull(config);
		Throw.IfNull(httpClient);
		Throw.IfNull(fileSystem);

		var logger = loggerFactory?.CreateLogger<StaticGenerator>();

		logger?.GeneratingStaticPages();
		logger?.Configuration(config, httpClient);

		Throw.InvalidOpWhen(
			() => config.OptimizePageContent && (config.OptimizerSelector is null),
			SR.Err_OptimizeWithoutChooser);

		if (!config.SkipPageResources && config.Pages.Any())
		{
			var pageResourceProcessorConfig =
				new PageResourceProcessorConfig(
					config.Pages,
					config.UpdateLinks,
					config.OptimizePageContent,
					config.OptimizerSelector,
					config.DestinationRoot,
					config.AlwaysCreateDefaultFile,
					config.IndexFileName,
					config.PageFileExtension,
					config.DefaultFileExclusions,
					httpClient, fileSystem, logger);

			foreach (var page in config.Pages)
			{
				await ProcessPageResource(page, pageResourceProcessorConfig, ct)
					.ConfigureAwait(false);
			}
		}

		if (config.OtherResources.Any())
		{
			var otherResourceProcessorConfig =
				new OtherResourceProcessorConfig(
					config.OptimizePageContent,
					config.OptimizerSelector,
					config.DestinationRoot,
					httpClient, fileSystem, logger);

			foreach (var resource in config.OtherResources)
			{
				if (config.SkipCssResources && resource is CssResource) continue;
				if (config.SkipJsResources && resource is JsResource) continue;
				if (config.SkipBinResources && resource is BinResource) continue;

				await ProcessOtherResource(resource, otherResourceProcessorConfig, ct)
					.ConfigureAwait(false);
			}
		}
	}

	public static async Task ExecuteForPage(
		PageResource page,
		StaticGeneratorConfig config,
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

		var logger = loggerFactory?.CreateLogger<StaticGenerator>();

		logger?.GeneratingStaticPages();
		logger?.ConfigurationSinglePage(page, config, httpClient);

		Throw.InvalidOpWhen(
			() => config.OptimizePageContent &&
			(config.OptimizerSelector is null),
			SR.Err_OptimizeWithoutChooser);


		await ProcessPageResource(
			page, new PageResourceProcessorConfig(
				config.Pages,
				config.UpdateLinks,
				config.OptimizePageContent,
				config.OptimizerSelector,
				config.DestinationRoot,
				config.AlwaysCreateDefaultFile,
				config.IndexFileName,
				config.PageFileExtension,
				config.DefaultFileExclusions,
				httpClient, fileSystem, logger),
			ct).ConfigureAwait(false);
	}


	[Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static async Task ProcessPageResource(
		PageResource page,
		PageResourceProcessorConfig config,
		CancellationToken ct = default)
	{
		Throw.IfNull(page);
		Throw.IfNull(config);

		var logger = config.Logger;

		if (ct.IsCancellationRequested) return;

		logger?.ProcessingResource(page);

		var outFilePathname = page.GetOutFilePathname(
			config.DestinationRoot,
			config.AlwaysCreateDefaultFile,
			config.IndexFileName,
			config.PageFileExtension,
			config.FileExclusions);

		var outFileShortName = outFilePathname.Replace(config.DestinationRoot, string.Empty);

		logger?.Debug("PagePath = {0}", outFileShortName);

		RemoveIfExists(config.FileSystem, outFilePathname);

		if (!EnsureFolderExists(config.FileSystem, outFilePathname))
		{
			logger?.FailedToEnsureFolder(outFilePathname);
			return;
		}

		var requestUri = page.Url;

		logger?.FetchingResourceContent(requestUri);

		var pageContent = string.Empty;

		try
		{
			pageContent = await config.HttpClient.GetStringAsync(
				requestUri, ct).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			logger?.Exception(ex);
		}

		if (string.IsNullOrWhiteSpace(pageContent))
		{
			logger?.NoContentForResource(requestUri);
			return;
		}

		if (config.UpdateLinks)
		{
			logger?.UpdatingHrefValues(outFileShortName);

			pageContent =
				pageContent.FixupHrefValues(
					config.AllPages,
					config.IndexFileName, config.PageFileExtension,
					config.AlwaysCreateDefaultFile);
		}

		if (config.OptimizeOutput && !page.SkipOptimization)
		{
			logger?.OptimizingContent(requestUri, outFilePathname);
			pageContent = OptimizeStringContent(page, pageContent, config.OptimizerSelector!, outFilePathname, requestUri, logger);

			#region OBSOLETE
			//var optimizer = config.OptimizerSelector!.SelectFor(page, outFilePathname);
			//logger?.SelectedOptimizer(requestUri, outFileShortName, optimizer.GetType().Name);
			//var result = optimizer.Minify(pageContent, page.OutputEncoding.ToSystemEncoding());
			//if (!result.Errors.Any())
			//{
			//	pageContent = result.MinifiedContent;
			//}
			//else
			//{
			//	logger?.OptimizationErrors(requestUri, outFileShortName, result.Errors);
			//}
			//if (result.Warnings.Any())
			//{
			//	logger?.OptimizationErrors(requestUri, outFileShortName, result.Warnings);
			//}
			#endregion
		}

		logger?.WritingContentToFile(requestUri, outFilePathname, pageContent.Length, page.OutputEncoding);

		await config.FileSystem.File
			.WriteAllTextAsync(outFilePathname, pageContent, page.OutputEncoding.ToSystemEncoding(), ct)
			.ConfigureAwait(false);
	}

	[Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static async Task ProcessOtherResource(
		NonPageResource resource,
		OtherResourceProcessorConfig config,
		CancellationToken ct = default)
	{
		Throw.IfNull(resource);
		Throw.IfNull(config);
		Throw.InvalidOpWhen(
			() => config.OptimizeOutput && (config.OptimizerSelector is null),
			$"Resource optimization requested but no {nameof(IOptimizerSelector)} registered.");

		var logger = config.Logger;

		if (ct.IsCancellationRequested) return;

		logger?.ProcessingResource(resource);

		var outFilePathname = resource.GetOutFilePathname(config.DestinationRoot);
		var outFileShortName = outFilePathname.Replace(config.DestinationRoot, string.Empty);

		logger?.Debug("OutFile = {0}", outFileShortName);

		//------------------------------------------------------------
		// Do not remove the existing resource file -- it may be 
		// the source for the served resource b/c the destination 
		// folder may be the same as the source folder for
		// the non-page resources (i.e. css or js foldrs in wwwroot).
		//------------------------------------------------------------
		//RemoveIfExists(config.FileSystem, outFilePathname);
		//------------------------------------------------------------

		if (!EnsureFolderExists(config.FileSystem, outFilePathname))
		{
			logger?.FailedToEnsureFolder(outFilePathname);
			return;
		}

		var requestUri = resource.Url;

		logger?.FetchingResourceContent(requestUri);

		await (resource switch
		{
			CssResource or JsResource => processStringResource(),
			BinResource => processBinaryResource((BinResource) resource),
			_ => Task.CompletedTask
		});


		async Task processStringResource()
		{
			var resourceContent = string.Empty;

			try
			{
				resourceContent = await config.HttpClient.GetStringAsync(
					requestUri, ct).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				logger?.Exception(ex);
			}

			if (string.IsNullOrWhiteSpace(resourceContent))
			{
				logger?.NoContentForResource(requestUri);
				return;
			}

			if (config.OptimizeOutput && !resource.SkipOptimization)
			{
				logger?.OptimizingContent(requestUri, outFileShortName);

				resourceContent = OptimizeStringContent(
					resource, resourceContent,
					config.OptimizerSelector!,
					outFilePathname, requestUri, logger);
			}

			logger?.WritingContentToFile(requestUri, outFileShortName, resourceContent.Length, resource.OutputEncoding);

			await config.FileSystem.File
				.WriteAllTextAsync(outFilePathname, resourceContent, resource.OutputEncoding.ToSystemEncoding(), ct)
				.ConfigureAwait(false);
		}

		async Task processBinaryResource(BinResource binResource)
		{
			var resourceContent = Array.Empty<byte>();

			try
			{
				resourceContent = await config.HttpClient.GetByteArrayAsync(
					requestUri, ct).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				logger?.Exception(ex);
			}

			if (0 == resourceContent.Length)
			{
				logger?.NoContentForResource(requestUri);
				return;
			}

			if (config.OptimizeOutput && !binResource.SkipOptimization)
			{
				var optimizer = config.OptimizerSelector!.SelectFor(binResource, outFilePathname);

				logger?.OptimizingContent(requestUri, outFileShortName);
				logger?.SelectedOptimizer(requestUri, outFileShortName, optimizer.GetType().Name);

				var opResult = optimizer.Execute(resourceContent, binResource, outFilePathname);

				if (!opResult.HasErrors)
				{
					resourceContent = opResult.OptimizedContent;
				}
				else
				{
					logger?.OptimizationErrors(requestUri, outFilePathname, [.. opResult.Errors]);
				}
				if (opResult.HasWarnings)
				{
					logger?.OptimizationWarnings(requestUri, outFilePathname, [.. opResult.Warnings]);
				}
			}

			logger?.WritingContentToFile(requestUri, outFilePathname, resourceContent.Length);

			await config.FileSystem.File
				.WriteAllBytesAsync(outFilePathname, resourceContent, ct)
				.ConfigureAwait(false);
		}
	}

	[Pure]
	private static string OptimizeStringContent(
		ResourceInfoBase resource, string content,
		IOptimizerSelector optimizerSelector,
		string outFilePathname, string requestUri,
		ILogger<StaticGenerator>? logger)
	{
		return resource switch
		{
			PageResource => optimizePageContent((PageResource) resource, content, optimizerSelector, outFilePathname, requestUri, logger),
			CssResource => optimizeCssContent((CssResource) resource, content, optimizerSelector, outFilePathname, requestUri, logger),
			JsResource => optimizeJsContent((JsResource) resource, content, optimizerSelector, outFilePathname, requestUri, logger),
			_ => content
		};


		static string optimizePageContent(
			PageResource resource, string content,
			IOptimizerSelector optimizerSelector,
			string outFilePathname, string requestUri,
			ILogger<StaticGenerator>? logger)
		{
			var optimizer = optimizerSelector.SelectFor(resource, outFilePathname);

			logger?.SelectedOptimizer(requestUri, outFilePathname, optimizer.GetType().Name);

			return
				getStringOptimizerResult(
					optimizer.Execute(content, resource, outFilePathname),
					outFilePathname, requestUri, logger) ?? content;
		}

		static string optimizeCssContent(
			CssResource resource, string content,
			IOptimizerSelector optimizerSelector,
			string outFilePathname, string requestUri,
			ILogger<StaticGenerator>? logger)
		{
			var optimizer = optimizerSelector.SelectFor(resource, outFilePathname);

			logger?.SelectedOptimizer(requestUri, outFilePathname, optimizer.GetType().Name);

			return
				getStringOptimizerResult(
					optimizer.Execute(content, resource, outFilePathname),
					outFilePathname, requestUri, logger) ?? content;
		}

		static string optimizeJsContent(
			JsResource resource, string content,
			IOptimizerSelector optimizerSelector,
			string outFilePathname, string requestUri,
			ILogger<StaticGenerator>? logger)
		{
			var optimizer = optimizerSelector.SelectFor(resource, outFilePathname);

			logger?.SelectedOptimizer(requestUri, outFilePathname, optimizer.GetType().Name);

			return
				getStringOptimizerResult(
					optimizer.Execute(content, resource, outFilePathname),
					outFilePathname, requestUri, logger) ?? content;
		}


		static string? getStringOptimizerResult(
			OptimizerResult<string> opResult,
			string outFilePathname, string requestUri,
			ILogger<StaticGenerator>? logger)
		{
			string? result = null;

			if (!opResult.HasErrors)
			{
				result = opResult.OptimizedContent;
			}
			else
			{
				logger?.OptimizationErrors(requestUri, outFilePathname, [.. opResult.Errors]);
			}
			if (opResult.HasWarnings)
			{
				logger?.OptimizationErrors(requestUri, outFilePathname, [.. opResult.Warnings]);
			}

			return result;
		}
	}


	private static void RemoveIfExists(IFileSystem fs, string pageDiskPath)
	{
		Throw.IfNull(fs);
		Throw.IfNullOrWhitespace(pageDiskPath);

		if (fs.File.Exists(pageDiskPath))
		{
			fs.File.Delete(pageDiskPath);
		}
	}

	private static bool EnsureFolderExists(IFileSystem fs, string pageDiskPath)
	{
		Throw.IfNull(fs);
		Throw.IfNullOrWhitespace(pageDiskPath);

		var folder = fs.Directory.GetParent(pageDiskPath);

		if (folder is null) return false;

		if (!folder.Exists)
		{
			folder?.Create();
		}

		return true;
	}


	#region local types

	private class PageResourceProcessorConfig(
		IEnumerable<PageResource> allPages,
		bool updateLinks,
		bool optimizeOutput,
		IOptimizerSelector? optimizerSelector,
		string destinationRoot,
		bool alwaysCreateDefaultFile,
		string indexFileName,
		string pageFileExtension,
		IEnumerable<string> fileExclusions,
		HttpClient httpClient,
		IFileSystem fileSystem,
		ILogger<StaticGenerator>? logger)
	{
		public IEnumerable<PageResource> AllPages { get; } = Throw.IfNull(allPages);
		public bool UpdateLinks { get; } = updateLinks;
		public bool OptimizeOutput { get; } = optimizeOutput;
		public IOptimizerSelector? OptimizerSelector { get; } = optimizerSelector;
		public string DestinationRoot { get; } = Throw.IfNullOrWhitespace(destinationRoot);
		public bool AlwaysCreateDefaultFile { get; } = alwaysCreateDefaultFile;
		public string IndexFileName { get; } = Throw.IfNullOrWhitespace(indexFileName);
		public string PageFileExtension { get; } = Throw.IfNullOrWhitespace(pageFileExtension);
		public string[] FileExclusions { get; } = Throw.IfNull(fileExclusions).ToArray();
		public HttpClient HttpClient { get; } = Throw.IfNull(httpClient);
		public IFileSystem FileSystem { get; } = Throw.IfNull(fileSystem);
		public ILogger<StaticGenerator>? Logger { get; } = logger;
	}


	private class OtherResourceProcessorConfig(
		bool optimizeOutput,
		IOptimizerSelector? optimizerSelector,
		string destinationRoot,
		HttpClient httpClient,
		IFileSystem fileSystem,
		ILogger<StaticGenerator>? logger)
	{
		public bool OptimizeOutput { get; } = optimizeOutput;
		public IOptimizerSelector? OptimizerSelector { get; } = optimizerSelector;
		public string DestinationRoot { get; } = Throw.IfNullOrWhitespace(destinationRoot);
		public HttpClient HttpClient { get; } = Throw.IfNull(httpClient);
		public IFileSystem FileSystem { get; } = Throw.IfNull(fileSystem);
		public ILogger<StaticGenerator>? Logger { get; } = logger;
	}

	#endregion
}


#region Logger Extensions...

internal static partial class StaticPageGeneratorLoggerExtensions
{
	#region 9001 - Configuration

	public static void Configuration(
		this ILogger<StaticGenerator> logger,
		StaticGeneratorConfig config,
		HttpClient httpClient) =>
		logger.Imp_Configuration(
			config.Pages.Count(),
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
		this ILogger<StaticGenerator> logger,
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
		this ILogger<StaticGenerator> logger,
		PageResource page,
		StaticGeneratorConfig config,
		HttpClient httpClient) =>
		logger.Imp_ConfigurationSinglePage(
			page.Url,
			config.Pages.Count(),
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
		this ILogger<StaticGenerator> logger,
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
		this ILogger<StaticGenerator> logger,
		string message,
		params object[] args) =>
		logger.Imp_Trace(
			string.Format(
				CultureInfo.InvariantCulture,
				message, args));

	[LoggerMessage(EventId = 1001, EventName = "Trace", Level = LogLevel.Trace,
		Message = "StaticPageGenerator: Trace > Message = {Message}")]
	private static partial void Imp_Trace(
		this ILogger<StaticGenerator> logger,
		string message);

	#endregion
	#region 1002 - Debug

	public static void Debug(
		this ILogger<StaticGenerator> logger,
		string message,
		params object[] args) =>
		logger.Imp_Debug(
			string.Format(
				CultureInfo.InvariantCulture,
				message, args));

	[LoggerMessage(EventId = 1002, EventName = "Debug", Level = LogLevel.Debug,
		Message = "StaticPageGenerator: Debug > Message = {Message}")]
	private static partial void Imp_Debug(
		this ILogger<StaticGenerator> logger,
		string message);

	#endregion
	#region 1003 - Exception

	public static void Exception(
		this ILogger<StaticGenerator> logger,
		Exception ex) =>
		logger.Imp_Exception(
			ex.Message,
			ex);

	[LoggerMessage(EventId = 1003, EventName = "Exception", Level = LogLevel.Critical,
		Message = "StaticPageGenerator: Exception > Error = {Error}")]
	private static partial void Imp_Exception(
		this ILogger<StaticGenerator> logger,
		string error,
		Exception ex);

	#endregion

	#region 1010 - GeneratingStaticPages

	public static void GeneratingStaticPages(
		this ILogger<StaticGenerator> logger) =>
		logger.Imp_GeneratingStaticPages();

	[LoggerMessage(EventId = 1010, EventName = "GeneratingStaticPages", Level = LogLevel.Information,
		Message = "StaticPageGenerator: Generating static pages...")]
	private static partial void Imp_GeneratingStaticPages(
		this ILogger<StaticGenerator> logger);

	#endregion

	#region 1020 - NoPagesToProcess

	public static void NoPagesToProcess(
		this ILogger<StaticGenerator> logger) =>
		logger.Imp_NoPagesToProcess();

	[LoggerMessage(EventId = 1020, EventName = "NoPagesToProcess", Level = LogLevel.Information,
		Message = "StaticPageGenerator: No pages to process. Exiting...")]
	private static partial void Imp_NoPagesToProcess(
		this ILogger<StaticGenerator> logger);

	#endregion

	#region 1030 - ProcessingResource

	private const string TR_Page = "Page";
	private const string TR_Css = "Page";
	private const string TR_Js = "Page";
	private const string TR_Bin = "Page";
	private const string TR_Other = "Other";

	public static void ProcessingResource(
		this ILogger<StaticGenerator> logger,
		ResourceInfoBase resource) =>
		logger.Imp_ProcessingResource(
			resource switch
			{
				PageResource => TR_Page,
				CssResource => TR_Css,
				JsResource => TR_Js,
				BinResource => TR_Bin,
				_ => TR_Other
			},
			resource.Route,
			resource.Query);

	[LoggerMessage(EventId = 1030, EventName = "ProcessingResource", Level = LogLevel.Information,
		Message = "StaticPageGenerator: Processing resource > ResourceType = {ResourceType}, Route = {Route}, Query = {Query}")]
	private static partial void Imp_ProcessingResource(
		this ILogger<StaticGenerator> logger,
		string resourceType,
		string route,
		string? query);

	#endregion

	#region 1040 - FailedToEnsureFolder

	public static void FailedToEnsureFolder(
		this ILogger<StaticGenerator> logger,
		string outFilePath) =>
		logger.Imp_FailedToEnsureFolder(
			outFilePath);

	[LoggerMessage(EventId = 1040, EventName = "FailedToEnsureFolder", Level = LogLevel.Error,
		Message = "StaticPageGenerator: Failed to ensure folder exists for resource > OutFilePath = {OutFilePath}")]
	private static partial void Imp_FailedToEnsureFolder(
		this ILogger<StaticGenerator> logger,
		string outFilePath);

	#endregion

	#region 1050 - FetchingResourceContent

	public static void FetchingResourceContent(
		this ILogger<StaticGenerator> logger,
		string resourceUrl) =>
		logger.Imp_FetchingResourceContent(
			resourceUrl);

	[LoggerMessage(EventId = 1050, EventName = "FetchingResourceContent", Level = LogLevel.Trace,
		Message = "StaticPageGenerator: Fetching resource content > ResourceUrl = {ResourceUrl}")]
	private static partial void Imp_FetchingResourceContent(
		this ILogger<StaticGenerator> logger,
		string resourceUrl);

	#endregion

	#region 1060 - UpdatingHrefValues

	public static void UpdatingHrefValues(
		this ILogger<StaticGenerator> logger,
		string pageFile) =>
		logger.Imp_UpdatingHrefValues(
			pageFile);

	[LoggerMessage(EventId = 1060, EventName = "UpdatingHrefValues", Level = LogLevel.Trace,
		Message = "StaticPageGenerator: Updating href values in page content > PageFile = {PageFile}")]
	private static partial void Imp_UpdatingHrefValues(
		this ILogger<StaticGenerator> logger,
		string pageFile);

	#endregion

	#region 1070 - NoContentForResource

	public static void NoContentForResource(
		this ILogger<StaticGenerator> logger,
		string resourceUrl) =>
		logger.Imp_NoContentForResource(
			resourceUrl);

	[LoggerMessage(EventId = 1070, EventName = "NoContentForResource", Level = LogLevel.Warning,
		Message = "StaticPageGenerator: No content for resource > ResourceUrl = {ResourceUrl}")]
	private static partial void Imp_NoContentForResource(
		this ILogger<StaticGenerator> logger,
		string resourceUrl);

	#endregion

	#region 1080 - OptimizingContent

	public static void OptimizingContent(
		this ILogger<StaticGenerator> logger,
		string resourceUrl,
		string outFilePath) =>
		logger.Imp_OptimizingContent(
			resourceUrl,
			outFilePath);

	[LoggerMessage(EventId = 1080, EventName = "OptimizingContent", Level = LogLevel.Information,
		Message = "StaticPageGenerator: Optimizing page content > ResourceUrl = {ResourceUrl}, outFilePath = {outFilePath}")]
	private static partial void Imp_OptimizingContent(
		this ILogger<StaticGenerator> logger,
		string resourceUrl,
		string outFilePath);

	#endregion

	#region 1082 - SelectedOptimizer

	public static void SelectedOptimizer(
		this ILogger<StaticGenerator> logger,
		string resourceUrl,
		string outFilePath,
		string optimizerName) =>
		logger.Imp_SelectedOptimizer(
			resourceUrl,
			outFilePath,
			optimizerName);

	[LoggerMessage(EventId = 1082, EventName = "SelectedOptimizer", Level = LogLevel.Debug,
		Message = "StaticPageGenerator: Selected optimizer > ResourceUrl = {ResourceUrl}, OutFilePath = {OutFilePath}, Optimizer = {Optimizer}")]
	private static partial void Imp_SelectedOptimizer(
		this ILogger<StaticGenerator> logger,
		string resourceUrl,
		string outFilePath,
		string optimizer);

	#endregion

	#region 1085 - OptimizationWarnings

	public static void OptimizationWarnings(
		this ILogger<StaticGenerator> logger,
		string resourceUrl,
		string outFilePath,
		OptimizerErrorInfo[] warnings) =>
		logger.Imp_OptimizationWarnings(
			resourceUrl,
			outFilePath,
			warnings);

	[LoggerMessage(EventId = 1085, EventName = "OptimizationWarnings", Level = LogLevel.Warning,
		Message = "StaticPageGenerator: Content optimizing warnings > ResourceUrl = {ResourceUrl}, OutFilePath = {OutFilePath}, Warnings = {Warnings}")]
	private static partial void Imp_OptimizationWarnings(
		this ILogger<StaticGenerator> logger,
		string resourceUrl,
		string outFilePath,
		OptimizerErrorInfo[] warnings);

	#endregion
	#region 1086 - OptimizationErrors

	public static void OptimizationErrors(
		this ILogger<StaticGenerator> logger,
		string resourceUrl,
		string outFilePath,
		OptimizerErrorInfo[] errors) =>
		logger.Imp_OptimizationErrors(
			resourceUrl,
			outFilePath,
			errors);

	[LoggerMessage(EventId = 1086, EventName = "OptimizationErrors", Level = LogLevel.Error,
		Message = "StaticPageGenerator: Content optimizing Errors > ResourceUrl = {ResourceUrl}, OutFilePath = {OutFilePath}, Errors = {Errors}")]
	private static partial void Imp_OptimizationErrors(
		this ILogger<StaticGenerator> logger,
		string resourceUrl,
		string outFilePath,
		OptimizerErrorInfo[] errors);

	#endregion

	#region 1088 - BinOptimizationWarnings

	//public static void BinOptimizationWarnings(
	//	this ILogger<StaticGenerator> logger,
	//	string resourceUrl,
	//	string outFilePath,
	//	OptimizerErrorInfo[] warnings) =>
	//	logger.Imp_BinOptimizationWarnings(
	//		resourceUrl,
	//		outFilePath,
	//		warnings);

	//[LoggerMessage(EventId = 1088, EventName = "BinOptimizationWarnings", Level = LogLevel.Warning,
	//	Message = "StaticPageGenerator: Content optimizing warnings > ResourceUrl = {ResourceUrl}, OutFilePath = {OutFilePath}, Warnings = {Warnings}")]
	//private static partial void Imp_BinOptimizationWarnings(
	//	this ILogger<StaticGenerator> logger,
	//	string resourceUrl,
	//	string outFilePath,
	//	OptimizerErrorInfo[] warnings);

	#endregion
	#region 1089 - BinOptimizationErrors

	//public static void BinOptimizationErrors(
	//	this ILogger<StaticGenerator> logger,
	//	string resourceUrl,
	//	string outFilePath,
	//	OptimizerErrorInfo[] errors) =>
	//	logger.Imp_BinOptimizationErrors(
	//		resourceUrl,
	//		outFilePath,
	//		errors);

	//[LoggerMessage(EventId = 1089, EventName = "BinOptimizationErrors", Level = LogLevel.Error,
	//	Message = "StaticPageGenerator: Content optimizing Errors > ResourceUrl = {ResourceUrl}, OutFilePath = {OutFilePath}, Errors = {Errors}")]
	//private static partial void Imp_BinOptimizationErrors(
	//	this ILogger<StaticGenerator> logger,
	//	string resourceUrl,
	//	string outFilePath,
	//	OptimizerErrorInfo[] errors);

	#endregion

	#region 1095 - WritingContentToFile

	public static void WritingContentToFile(
		this ILogger<StaticGenerator> logger,
		string resourceUrl,
		string outFilePath,
		int contentSize,
		EncodingType? encoding = default) =>
		logger.Imp_WritingContentToFile(
			resourceUrl,
			outFilePath,
			contentSize,
			encoding ?? EncodingType.Default);

	[LoggerMessage(EventId = 1095, EventName = "WritingContentToFile", Level = LogLevel.Information,
		Message = "StaticPageGenerator: Writing resource content to file > ResourceUrl = {ResourceUrl}, OutFilePath = {OutFilePath}, ContentSize = {ContentSize}, Encoding = {Encoding}")]
	private static partial void Imp_WritingContentToFile(
		this ILogger<StaticGenerator> logger,
		string resourceUrl,
		string outFilePath,
		int contentSize,
		EncodingType encoding);

	#endregion
}

#endregion
