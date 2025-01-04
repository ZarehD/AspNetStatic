namespace Tests.AspNetStatic;

[TestClass]
public class RouteToPathnameTests
{
	private static readonly string _webroot = "\\root";
	private static readonly string _indexFileName = "index.html";
	private static readonly string _pageFileExtension = ".html";
	private static readonly string[] _exclusions = ["index", "default"];

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

	#region GetPathname v1

	[DataTestMethod]
	[DataRow("file.css", @"\root\file.css")]
	[DataRow("/file.css", @"\root\file.css")]
	[DataRow("file.js", @"\root\file.js")]
	[DataRow("/file.js", @"\root\file.js")]
	[DataRow("file.bin", @"\root\file.bin")]
	[DataRow("/file.bin", @"\root\file.bin")]
	[DataRow("/segment/file.jpg", @"\root\segment\file.jpg")]
	[DataRow("/segment/file.jpg", @"\root\segment\file.jpg")]
	public void GetPathname_v1(string route, string expected)
	{
		var resource = new NonPageResource(route);
		expected = expected
			.Replace(Consts.BakSlash, Path.DirectorySeparatorChar)
			.Replace(Consts.FwdSlash, Path.DirectorySeparatorChar);

		var actual = resource.GetOutFilePathname(_webroot.ToFileSysPath());

		Assert.AreEqual(expected, actual, ignoreCase: true);
	}

	[DataTestMethod]
	[DataRow("/file.xyz", null, @"\root\file.xyz")]
	[DataRow("/file.xyz", "", @"\root\file.xyz")]
	[DataRow("/file.xyz", " ", @"\root\file.xyz")]
	[DataRow("/file.xyz", "xyz-file.xyz", @"\root\xyz-file.xyz")]
	[DataRow("/file.xyz", "my-file.xyz", @"\root\my-file.xyz")]
	[DataRow("/folder/file.xyz", @"\folder/my-file.xyz", @"/root/folder\my-file.xyz")]
	public void GetPathname_v1_Override(string route, string? overridePathname, string expected)
	{
		var resource = new NonPageResource(route)
		{
			OutFile = overridePathname
		};
		expected = expected
			.Replace(Consts.BakSlash, Path.DirectorySeparatorChar)
			.Replace(Consts.FwdSlash, Path.DirectorySeparatorChar);

		var actual = resource.GetOutFilePathname(_webroot.ToFileSysPath());

		Assert.AreEqual(expected, actual, ignoreCase: true);
	}

	[TestMethod]
	public void GetPathname_v1_Null_Page_Shd_Throw()
	{
		var resource = default(NonPageResource);

		var action = () => resource!.GetOutFilePathname(_webroot);

		Assert.ThrowsException<ArgumentNullException>(action);
	}

	[TestMethod]
	public void GetPathname_v1_Bad_RootFolder_Shd_Throw()
	{
		var resource = new NonPageResource("/file.xyz");

		Assert.ThrowsException<ArgumentNullException>(
			() => resource.GetOutFilePathname(_nullString));

		Assert.ThrowsException<ArgumentException>(
			() => resource.GetOutFilePathname(_emptyString));

		Assert.ThrowsException<ArgumentException>(
			() => resource.GetOutFilePathname(_whitespace));
	}

	#endregion

	#region GetPathname v2

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
	public void GetPathname_v2(string route, bool createDefaultFile, string expected)
	{
		var page = new PageResource(route);
		expected = expected
			.Replace(Consts.BakSlash, Path.DirectorySeparatorChar)
			.Replace(Consts.FwdSlash, Path.DirectorySeparatorChar);

		var actual = page.GetOutFilePathname(
			_webroot.ToFileSysPath(), createDefaultFile,
			_indexFileName, _pageFileExtension,
			_exclusions);

		Assert.AreEqual(expected, actual, ignoreCase: true);
	}

	[DataTestMethod]
	[DataRow("/", null, @"\root\index.html")]
	[DataRow("/", "", @"\root\index.html")]
	[DataRow("/", " ", @"\root\index.html")]
	[DataRow("/", "page.htm", @"\root\page.htm")]
	[DataRow("/page", "my-page.htm", @"\root\my-page.htm")]
	[DataRow("/page/", @"\page/default.htm", @"/root/page\default.htm")]
	public void GetPathname_v2_Override(string route, string? overridePathname, string expected)
	{
		var page = new PageResource(route)
		{
			OutFile = overridePathname
		};

		expected = expected
			.Replace(Consts.BakSlash, Path.DirectorySeparatorChar)
			.Replace(Consts.FwdSlash, Path.DirectorySeparatorChar);

		var actual = page.GetOutFilePathname(
			_webroot.ToFileSysPath(), false,
			_indexFileName, _pageFileExtension, _exclusions);

		Assert.AreEqual(expected, actual, ignoreCase: false);
	}

	[TestMethod]
	public void GetPathname_v2_Null_Page_Shd_Throw()
	{
		var page = default(PageResource);

		var action = () => page!.GetOutFilePathname(
			_webroot, false,
			_indexFileName, _pageFileExtension,
			_exclusions);

		Assert.ThrowsException<ArgumentNullException>(action);
	}

	[TestMethod]
	public void GetPathname_v2_Bad_RootFolder_Shd_Throw()
	{
		var page = new PageResource("/");

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
	public void GetPathname_v2_Bad_IndexFileName_Shd_Throw()
	{
		var page = new PageResource("/");

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
	public void GetPathname_v2_Bad_PageFileExtension_Shd_Throw()
	{
		var page = new PageResource("/");

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
