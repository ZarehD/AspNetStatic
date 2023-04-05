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
			});

		private class PageInfoProvider : StaticPagesInfoProviderBase
		{
			public PageInfoProvider()
			{
				this.pages.AddRange(_pages);
			}
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
				webRoot, filePathname.AssureNotStartsWith(
					RouteConsts.BakSlash));
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
				webRoot, filePathname.AssureNotStartsWith(
					RouteConsts.BakSlash));

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
		public async Task Test_Should_fallback_to_correct_path(
			string requestPath, string expectedPath,
			bool alwaysDefaultfile)
		{
			var httpContextMoq = new Mock<HttpContext>();
			httpContextMoq
				.Setup(x => x.Request.Path)
				.Returns(new PathString(requestPath));
			httpContextMoq
				.SetupSet(x => x.Request.Path = new PathString(expectedPath))
				.Verifiable();

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
				!expectedPath.EndsWith(RouteConsts.FwdSlash) &&
				Path.HasExtension(expectedPath) &&
				pageInfoProvider.Pages.Any(p => p.Route.Equals(requestPath))
				;

			if (createFile)
			{
				pathname = expectedPath.Replace(RouteConsts.FwdSlash, Path.DirectorySeparatorChar);
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
