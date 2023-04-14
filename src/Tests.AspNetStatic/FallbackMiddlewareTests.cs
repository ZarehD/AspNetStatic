using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.AspNetStatic
{
	[TestClass]
	public class FallbackMiddlewareTests
	{
		private static readonly List<PageInfo> _pages =
			new(new PageInfo[]
			{
				new("/"),
				new("/blog/"),
				new("/blog/article1"),
				new("/doc/p1") { OutFile = "doc\\page1.htm" },
				new("/doc/p2/123") { OutFile = "doc\\page2-123.htm" },
				new("/doc/p3") { Query = "?p1=v1&p2=v2", OutFile = "doc\\page3-p1v1-p2v2.htm" },
				new("/doc/p4/567") { Query = "?p1=v1", OutFile = "doc\\page4-567-p1v1.htm" },
				new("/doc/p4/789/") { Query = "?p1=v1", OutFile = "doc\\page4-789-p1v1.htm" },
			});

		private class PageInfoProvider : StaticPagesInfoProvider
		{
			public PageInfoProvider() : base(_pages) { }
		}

		private static ILogger<StaticPageFallbackMiddleware> GetMiddlewareLogger() =>
			new Mock<ILogger<StaticPageFallbackMiddleware>>().Object;

		private static RequestDelegate GetNextMiddleware() => new((r) => Task.CompletedTask);

		private static IStaticPagesInfoProvider GetPagenfoProvider() => new PageInfoProvider();

		private static IWebHostEnvironment GetWebHostEnvironment()
		{
			var moqEnv = new Mock<IWebHostEnvironment>();
			moqEnv.Setup(x => x.WebRootPath).Returns(
				Path.Combine(Directory.GetCurrentDirectory(), "webroot"));
			return moqEnv.Object;
		}

		private static void CreateFileInWebRoot(string filePathname)
		{
			var webRoot = GetWebHostEnvironment().WebRootPath;
			var fullPathname = Path.Combine(
				webRoot, filePathname.EnsureNotStartsWith(
					Consts.BakSlash));
			if (!Directory.Exists(Path.GetDirectoryName(fullPathname)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(fullPathname)!);
			}
			File.WriteAllText(fullPathname, "AspNetStatic fallback middleware test content.", Encoding.UTF8);
		}

		private static void DeleteFile(string filePathname)
		{
			if (string.IsNullOrWhiteSpace(filePathname)) return;

			var webRoot = GetWebHostEnvironment().WebRootPath;
			var fullPathname = Path.Combine(
				webRoot, filePathname.EnsureNotStartsWith(
					Consts.BakSlash));

			if (File.Exists(fullPathname))
			{
				File.Delete(fullPathname);
			}
		}



		[DataTestMethod]
		[DataRow("/blog/article1", "/blog/article1.html", false)]
		[DataRow("/blog/article1", "/blog/article1/index.html", true)]
		[DataRow("/blog/article1/", "/blog/article1/index.html", false)]
		[DataRow("/page/", "/page/", false)]
		[DataRow("/page/p1", "/page/p1", true)]
		public async Task Test_Fallbacks_AutoPath(
			string requestPath, string expectedPath,
			bool alwaysDefaultfile)
		{
			var parts = requestPath.Split('?');
			var path = parts[0];
			var query = parts.Length > 1 ? parts[1] : null;

			var httpContextMoq = new Mock<HttpContext>();

			httpContextMoq.Setup(x => x.Request.Headers).Returns(new HeaderDictionary());
			httpContextMoq.Setup(x => x.Request.Path).Returns(new PathString(path));
			httpContextMoq.Setup(x => x.Request.QueryString).Returns(new QueryString(query.EnsureStartsWith('?', true)));
			httpContextMoq.SetupSet(x => x.Request.Path = new PathString(expectedPath)).Verifiable();

			var httpContext = httpContextMoq.Object;

			var pageInfoProvider = GetPagenfoProvider();

			var sut = new StaticPageFallbackMiddleware(
				GetMiddlewareLogger(),
				GetNextMiddleware(),
				pageInfoProvider,
				GetWebHostEnvironment(),
				new()
				{
					AlwaysDefaultFile = alwaysDefaultfile
				});

			var pathname = string.Empty;
			var createFile =
				!expectedPath.EndsWith(Consts.FwdSlash) &&
				Path.HasExtension(expectedPath) &&
				pageInfoProvider.Pages.ContainsPageForUrl(requestPath)
				;

			if (createFile)
			{
				pathname = expectedPath.Replace(Consts.FwdSlash, Path.DirectorySeparatorChar);
				CreateFileInWebRoot(pathname);
			}
			try
			{
				await sut.InvokeAsync(httpContext);
			}
			finally
			{
				if (createFile) DeleteFile(pathname);
			}

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
		[DataRow("/doc/p1", "/doc/page1.htm", false)]
		[DataRow("/doc/", "/doc/index.html", true)]
		[DataRow("/doc/p1", "/doc/p1.html", true)]
		[DataRow("/doc/p2/123", "/doc/page2-123.htm", false)]
		[DataRow("/doc/p3?p1=v1&p2=v2", "/doc/page3-p1v1-p2v2.htm", false)]
		[DataRow("/doc/p4/567/?p1=v1", "/doc/page4-567-p1v1.htm", false)]
		[DataRow("/doc/p4/567?p1=v1", "/doc/p4/567.html", true)]
		[DataRow("/doc/p4/789/?p1=v1", "/doc/page4-789-p1v1.htm", false)]
		[DataRow("/doc/p4/789/?p1=v1", "/doc/p4/789/index.html", true)]
		public async Task Test_Fallback_OutFilePathname(
			string requestPath, string expectedPath,
			bool ignoreOutFilePathname)
		{
			var parts = requestPath.Split('?');
			var path = parts[0];
			var query = parts.Length > 1 ? parts[1] : null;

			var httpContextMoq = new Mock<HttpContext>();

			httpContextMoq.Setup(x => x.Request.Headers).Returns(new HeaderDictionary());
			httpContextMoq.Setup(x => x.Request.Path).Returns(new PathString(path));
			httpContextMoq.Setup(x => x.Request.QueryString).Returns(new QueryString(query.EnsureStartsWith('?', true)));
			httpContextMoq.SetupSet(x => x.Request.Path = new PathString(expectedPath)).Verifiable();

			var httpContext = httpContextMoq.Object;

			var pageInfoProvider = GetPagenfoProvider();

			var sut = new StaticPageFallbackMiddleware(
				GetMiddlewareLogger(),
				GetNextMiddleware(),
				pageInfoProvider,
				GetWebHostEnvironment(),
				new()
				{
					IgnoreOutFilePathname = ignoreOutFilePathname
				});

			var pathname = string.Empty;
			var createFile =
				!expectedPath.EndsWith(Consts.FwdSlash) &&
				Path.HasExtension(expectedPath) &&
				pageInfoProvider.Pages.ContainsPageForUrl(requestPath)
				;

			if (createFile)
			{
				pathname = expectedPath.Replace(Consts.FwdSlash, Path.DirectorySeparatorChar);
				CreateFileInWebRoot(pathname);
			}
			try
			{
				await sut.InvokeAsync(httpContext);
			}
			finally
			{
				if (createFile) DeleteFile(pathname);
			}

			if (createFile)
			{
				httpContextMoq.VerifySet(x => x.Request.Path = new PathString(expectedPath)); //new PathString(expectedPath)
			}
			else
			{
				httpContextMoq.VerifySet(x => x.Request.Path = It.IsAny<PathString>(), Times.Never);
			}
		}
	}
}
