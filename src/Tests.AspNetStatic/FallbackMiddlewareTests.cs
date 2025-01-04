using System.IO.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Tests.AspNetStatic;

[TestClass]
public class FallbackMiddlewareTests
{
	private const string C_File_Doc_P1 = "doc/page1.html";
	private const string C_File_Doc_P2_123 = @"doc\page2-123.htm";
	private const string C_File_Doc_P3 = "doc/page3-p1v1-p2v2.htm";
	private const string C_File_Doc_P4_567_P1V1 = "doc/page4-567-p1v1.htm";
	private const string C_File_Doc_P4_789_P1V1 = "doc/page4-789-p1v1.html";

	private static readonly string File_Doc_P1 = C_File_Doc_P1.ToFileSysPath();
	private static readonly string File_Doc_P2_123 = C_File_Doc_P2_123.ToFileSysPath();
	private static readonly string File_Doc_P3 = C_File_Doc_P3.ToFileSysPath();
	private static readonly string File_Doc_P4_567_P1V1 = C_File_Doc_P4_567_P1V1.ToFileSysPath();
	private static readonly string File_Doc_P4_789_P1V1 = C_File_Doc_P4_789_P1V1.ToFileSysPath();

	private static readonly List<PageResource> _pages =
		new(
		[
			new("/"),
			new("/blog/"),
			new("/blog/article1"),
			new("/doc/page-a"),
			new("/doc/page-b") { OutFile = "/doc/page-b/index.html" },
			new("/doc/p1") { OutFile = File_Doc_P1 },
			new("/doc/p2/123") { OutFile = File_Doc_P2_123 },
			new("/doc/p3") { Query = "?p1=v1&p2=v2", OutFile = File_Doc_P3 },
			new("/doc/p4/567") { Query = "?p1=v1", OutFile = File_Doc_P4_567_P1V1 },
			new("/doc/p4/789/") { Query = "?p1=v1", OutFile = File_Doc_P4_789_P1V1 },
		]);

	private class PageInfoProvider : StaticResourcesInfoProvider
	{
		public PageInfoProvider() : base(_pages, defaultFileExtension: ".html") { }
	}

	private static ILogger<StaticPageFallbackMiddleware> GetMiddlewareLogger() =>
		new Mock<ILogger<StaticPageFallbackMiddleware>>().Object;

	private static RequestDelegate GetNextMiddleware() => new((r) => Task.CompletedTask);

	private static IStaticResourcesInfoProvider GetPageInfoProvider() => new PageInfoProvider();

	private static IWebHostEnvironment GetWebHostEnvironment()
	{
		var moqEnv = new Mock<IWebHostEnvironment>();
		moqEnv.Setup(x => x.WebRootPath).Returns(
			Path.Combine(Directory.GetCurrentDirectory(), "webroot"));
		return moqEnv.Object;
	}

	private static string GetOutFileFullPath(string pageRoute) =>
		Path.Combine(
			GetWebHostEnvironment().WebRootPath, pageRoute
			.Replace(Consts.FwdSlash, Path.DirectorySeparatorChar)
			.Replace(Consts.BakSlash, Path.DirectorySeparatorChar)
			.EnsureNotStartsWith(Path.DirectorySeparatorChar));



	[DataTestMethod]
	[DataRow("/blog/article1", "/blog/article1.html", false)]
	[DataRow("/blog/article1", "/blog/article1/index.html", true)]
	[DataRow("/blog/article1/", "/blog/article1/index.html", false)]
	[DataRow("/page/", "/page/", false)]
	[DataRow("/page/p1", "/page/p1", true)]
	public async Task Test_Fallbacks_AutoPath(
		string requestPath, string expectedPath,
		bool alwaysDefaultFile)
	{
		// Setup...
		//
		var parts = requestPath.EnsureStartsWith(Consts.FwdSlash).Split('?');
		var path = parts[0];
		var query = parts.Length > 1 ? parts[1] : null;

		var pageInfoProvider = GetPageInfoProvider();

		var createFile =
			!expectedPath.EndsWith(Consts.FwdSlash) &&
			Path.HasExtension(expectedPath) &&
			pageInfoProvider.PageResources.ContainsResourceForUrl(requestPath);

		var diskFilePathname = GetOutFileFullPath(expectedPath);

		var fileSystemMoq = new Mock<IFileSystem>();
		fileSystemMoq.Setup(x => x.File.Exists(diskFilePathname)).Returns(true);
		var fileSystem = fileSystemMoq.Object;

		expectedPath = expectedPath.EnsureStartsWith(Consts.FwdSlash);
		var httpContextMoq = new Mock<HttpContext>();
		httpContextMoq.Setup(x => x.Request.Headers).Returns(new HeaderDictionary());
		httpContextMoq.Setup(x => x.Request.Path).Returns(new PathString(path));
		httpContextMoq.Setup(x => x.Request.QueryString).Returns(new QueryString(query.EnsureStartsWith('?', true)));
		httpContextMoq.SetupSet(x => x.Request.Path = new PathString(expectedPath)).Verifiable();
		var httpContext = httpContextMoq.Object;

		var sut = new StaticPageFallbackMiddleware(
			GetMiddlewareLogger(),
			fileSystem,
			GetNextMiddleware(),
			pageInfoProvider,
			GetWebHostEnvironment(),
			Options.Create<StaticPageFallbackMiddlewareOptions>(
				new()
				{
					AlwaysDefaultFile = alwaysDefaultFile
				}));


		// Act...
		//
		await sut.InvokeAsync(httpContext);


		// Assess...
		//
		if (createFile)
		{
			httpContextMoq.VerifySet(x => x.Request.Path = new PathString(expectedPath)); //new PathString(expectedPath)
		}
		else
		{
			httpContextMoq.VerifySet(x => x.Request.Path = It.IsAny<PathString>(), Times.Never);
		}
	}


	[DataTestMethod]
	[DataRow("/doc/", "/doc/index.html", true)]
	[DataRow("/doc/p1", C_File_Doc_P1, false)]
	[DataRow("/doc/p1", "/doc/p1.html", true)]
	[DataRow("/doc/p2/123", "/doc/page2-123.htm", false)]
	[DataRow("/doc/p3?p1=v1&p2=v2", "/doc/page3-p1v1-p2v2.htm", false)]
	[DataRow("/doc/p4/567/?p1=v1", "/doc/page4-567-p1v1.htm", false)]
	[DataRow("/doc/p4/567?p1=v1", "/doc/p4/567.html", true)]
	[DataRow("/doc/p4/789/?p1=v1", "/doc/page4-789-p1v1.html", false)]
	[DataRow("/doc/p4/789/?p1=v1", "/doc/p4/789/index.html", true)]
	public async Task Test_Fallback_OutFilePathname(
		string requestPath, string expectedPath,
		bool ignoreOutFilePathname)
	{
		// Setup...
		//
		var parts = requestPath.EnsureStartsWith(Consts.FwdSlash).Split('?');
		var path = parts[0];
		var query = parts.Length > 1 ? parts[1] : null;

		var pageInfoProvider = GetPageInfoProvider();

		var createFile =
			!expectedPath.EndsWith(Consts.FwdSlash) &&
			Path.HasExtension(expectedPath) &&
			pageInfoProvider.PageResources.ContainsResourceForUrl(requestPath);

		var diskFilePathname = GetOutFileFullPath(expectedPath);

		var fileSystemMoq = new Mock<IFileSystem>();
		fileSystemMoq.Setup(x => x.File.Exists(diskFilePathname)).Returns(true);
		var fileSystem = fileSystemMoq.Object;

		expectedPath = expectedPath.EnsureStartsWith(Consts.FwdSlash);
		var httpContextMoq = new Mock<HttpContext>();
		httpContextMoq.Setup(x => x.Request.Headers).Returns(new HeaderDictionary());
		httpContextMoq.Setup(x => x.Request.Path).Returns(new PathString(path));
		httpContextMoq.Setup(x => x.Request.QueryString).Returns(new QueryString(query.EnsureStartsWith('?', true)));
		httpContextMoq.SetupSet(x => x.Request.Path = new PathString(expectedPath)).Verifiable();
		var httpContext = httpContextMoq.Object;

		var sut = new StaticPageFallbackMiddleware(
			GetMiddlewareLogger(),
			fileSystem,
			GetNextMiddleware(),
			pageInfoProvider,
			GetWebHostEnvironment(),
			Options.Create<StaticPageFallbackMiddlewareOptions>(
				new()
				{
					IgnoreOutFilePathname = ignoreOutFilePathname
				}));


		// Act...
		//
		await sut.InvokeAsync(httpContext);


		// Assess...
		//
		if (createFile)
		{
			httpContextMoq.VerifySet(x => x.Request.Path = new PathString(expectedPath)); //new PathString(expectedPath)
		}
		else
		{
			httpContextMoq.VerifySet(x => x.Request.Path = It.IsAny<PathString>(), Times.Never);
		}
	}


	[DataTestMethod]
	[DataRow("/doc/page-a.html", "/doc/page-a", false)]
	[DataRow("/doc/page-b/index.html", "/doc/page-b", true)]
	[DataRow(C_File_Doc_P1, "/doc/p1", false)]
	[DataRow(C_File_Doc_P2_123, "/doc/p2/123", false)]
	[DataRow($"{C_File_Doc_P3}?p1=v1&p2=v2", "/doc/p3", false)]
	[DataRow($"{C_File_Doc_P4_567_P1V1}?p1=v1", "/doc/p4/567", false)]
	[DataRow($"{C_File_Doc_P4_789_P1V1}?p1=v1", "/doc/p4/789/", false)]
	public async Task Test_Reverse_Fallback_AutoPath(
		string requestPath, string expectedPath,
		bool alwaysDefaultFile)
	{
		// Setup...
		//
		var parts = requestPath.EnsureStartsWith(Consts.FwdSlash).Split('?');
		var path = parts[0];
		var query = parts.Length > 1 ? parts[1] : null;

		var pageInfoProvider = GetPageInfoProvider();

		var diskFilePathname = GetOutFileFullPath(path);

		var fileSystemMoq = new Mock<IFileSystem>();
		fileSystemMoq.Setup(x => x.File.Exists(diskFilePathname)).Returns(false);
		var fileSystem = fileSystemMoq.Object;

		expectedPath = expectedPath.EnsureStartsWith(Consts.FwdSlash);
		var httpContextMoq = new Mock<HttpContext>();
		httpContextMoq.Setup(x => x.Request.Headers).Returns(new HeaderDictionary());
		httpContextMoq.Setup(x => x.Request.Path).Returns(new PathString(path));
		httpContextMoq.Setup(x => x.Request.QueryString).Returns(new QueryString(query.EnsureStartsWith('?', true)));
		httpContextMoq.SetupSet(x => x.Request.Path = new PathString(expectedPath)).Verifiable();
		var httpContext = httpContextMoq.Object;

		var sut = new StaticPageFallbackMiddleware(
			GetMiddlewareLogger(),
			fileSystem,
			GetNextMiddleware(),
			pageInfoProvider,
			GetWebHostEnvironment(),
			Options.Create<StaticPageFallbackMiddlewareOptions>(
				new()
				{
					AlwaysDefaultFile = alwaysDefaultFile
				}));


		// Act...
		//
		await sut.InvokeAsync(httpContext);


		// Assess...
		//
		var expectedQuery = $"{expectedPath}{query.EnsureStartsWith("?", true)}";
		httpContextMoq.VerifySet(x => x.Request.Path = new PathString(expectedQuery)); //new PathString(expectedPath)
	}
}
