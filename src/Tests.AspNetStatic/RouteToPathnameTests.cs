namespace Tests.AspNetStatic
{
	[TestClass]
	public class RouteToPathnameTests
	{
		private static readonly string _webroot = "\\root";
		private static readonly string _indexFileName = "index.html";
		private static readonly string _pageFileExtension = ".html";
		private static readonly string[] _exclusions = new[] { "index", "default" };

		private static readonly string _nullString = default!;
		private static readonly string _emptyString = string.Empty;
		private static readonly string _whitespace = " ";



		#region GetUrlWithoutQueryString

		[DataTestMethod]
		[DataRow("/", "/")]
		[DataRow("/page", "/page")]
		[DataRow("/page/", "/page/")]
		[DataRow("/page?q1=v1", "/page")]
		[DataRow("/segment/page?q1=v1", "/segment/page")]
		[DataRow("/page/?q1=v1", "/page/")]
		[DataRow("/segment/page/?q1=v1", "/segment/page/")]
		public void Test_GetUrlWithoutQueryString(string url, string expected)
		{
			var actual = url.StripQueryString();
			Assert.AreEqual(expected, actual, ignoreCase: true);
		}

		[DataTestMethod]
		[DataRow(default(string))]
		[DataRow("")]
		[DataRow(" ")]
		public void Test_GetUrlWithoutQueryString_BadInput(string url)
		{
			//Assert.ThrowsException<ArgumentException>(() => RouteToPathname.StripQueryString(url));
			Assert.AreEqual(url, url.StripQueryString(), ignoreCase: true);
		}

		#endregion

		#region GetPathname

		[DataTestMethod]
		[DataRow("/", false, @"\root\index.html")]
		[DataRow("/", true, @"\root\index.html")]
		[DataRow("/index", false, @"\root\index.html")]
		[DataRow("/index", true, @"\root\index.html")]
		[DataRow("/index/", false, @"\root\index\index.html")]
		[DataRow("/index/", true, @"\root\index\index.html")]
		[DataRow("/segment/index", false, @"\root\segment\index.html")]
		[DataRow("/segment/index", true, @"\root\segment\index.html")]
		[DataRow("/page", false, @"\root\page.html")]
		[DataRow("/page", true, @"\root\page\index.html")]
		[DataRow("/segment/page", false, @"\root\segment\page.html")]
		[DataRow("/segment/page", true, @"/root/segment\page\index.html")]
		public void Test_GetPathname(string route, bool createDefaultFile, string expected)
		{
			var page = new PageInfo(route);

			var actual = page.GetOutFilePathname(
				_webroot.ToFileSysPath(), createDefaultFile,
				_indexFileName, _pageFileExtension,
				_exclusions);

			expected = expected
				.Replace(Consts.BakSlash, Path.DirectorySeparatorChar)
				.Replace(Consts.FwdSlash, Path.DirectorySeparatorChar);

			Assert.AreEqual(expected, actual, ignoreCase: true);
		}


		[DataTestMethod]
		[DataRow("/", null, @"\root\index.html")]
		[DataRow("/", "", @"\root\index.html")]
		[DataRow("/", " ", @"\root\index.html")]
		[DataRow("/", "page.htm", @"\root\page.htm")]
		[DataRow("/page", "my-page.htm", @"\root\my-page.htm")]
		[DataRow("/page/", @"\page/default.htm", @"/root/page\default.htm")]
		public void Test_GetPathname_Override(string route, string? overridePathname, string expected)
		{
			var page = new PageInfo(route)
			{
				OutFile = overridePathname
			};

			var actual = page.GetOutFilePathname(
				_webroot.ToFileSysPath(), false,
				_indexFileName, _pageFileExtension, _exclusions);

			expected = expected
				.Replace(Consts.BakSlash, Path.DirectorySeparatorChar)
				.Replace(Consts.FwdSlash, Path.DirectorySeparatorChar);

			Assert.AreEqual(expected, actual, ignoreCase: true);
		}


		[TestMethod]
		public void Test_GetPathname_BadInput_Page()
		{
			var page = default(PageInfo);

			Assert.ThrowsException<ArgumentNullException>(
				() => page!.GetOutFilePathname(
					_webroot, false,
					_indexFileName, _pageFileExtension,
					_exclusions));
		}

		[TestMethod]
		public void Test_GetPathname_BadInput_RootFolder()
		{
			var page = new PageInfo("/");

			Assert.ThrowsException<ArgumentNullException>(
				() => page.GetOutFilePathname(
					_nullString, false,
					_indexFileName, _pageFileExtension,
					_exclusions));

			Assert.ThrowsException<ArgumentException>(
				() => page.GetOutFilePathname(
					_emptyString, false,
					_indexFileName, _pageFileExtension,
					_exclusions));

			Assert.ThrowsException<ArgumentException>(
				() => page.GetOutFilePathname(
					_whitespace, false,
					_indexFileName, _pageFileExtension,
					_exclusions));
		}

		[TestMethod]
		public void Test_GetPathname_BadInput_IndexFileName()
		{
			var page = new PageInfo("/");

			Assert.ThrowsException<ArgumentNullException>(
				() => page.GetOutFilePathname(
					_webroot, false,
					_nullString, _pageFileExtension,
					_exclusions));

			Assert.ThrowsException<ArgumentException>(
				() => page.GetOutFilePathname(
					_webroot, false,
					_emptyString, _pageFileExtension,
					_exclusions));

			Assert.ThrowsException<ArgumentException>(
				() => page.GetOutFilePathname(
					_webroot, false,
					_whitespace, _pageFileExtension,
					_exclusions));
		}

		[TestMethod]
		public void Test_GetPathname_BadInput_PageFileExtension()
		{
			var page = new PageInfo("/");

			Assert.ThrowsException<ArgumentNullException>(
				() => page.GetOutFilePathname(
					_webroot, false,
					_indexFileName, _nullString,
					_exclusions));

			Assert.ThrowsException<ArgumentException>(
				() => page.GetOutFilePathname(
					_webroot, false,
					_indexFileName, _emptyString,
					_exclusions));

			Assert.ThrowsException<ArgumentException>(
				() => page.GetOutFilePathname(
					_webroot, false,
					_indexFileName, _whitespace,
					_exclusions));
		}

		#endregion
	}
}
