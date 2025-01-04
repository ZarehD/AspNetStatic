namespace Tests.AspNetStatic;

[TestClass]
public class PageInfoExtensionsTests
{
	private static readonly List<PageResource> _pages =
		new(
		[
			new("/"),
			new("/privacy"),
			new("/Policies/"),
			new("/blog/"),
			new("/blog/article1"),
			new("/blog/categories/"),
			new("/blogs/blog/article2"),
			new("/blogs/blog/categories/"),
			new("/blogs/blog/articles/"),
			new("/blogs/blog/articles/article1"),
			new("docs/"),
			new("docs/page1"),
			new("docs/page2/"),
			new("/segment/page/1") { Query="?p1=v1" },
			new("/segment/page/2/") { Query="p1=v1" },
		]);


	[DataTestMethod]
	[DataRow("abc/", false, false)]
	[DataRow("/abc", false, false)]
	[DataRow("/abc/", false, false)]
	[DataRow("/abc/xyz", false, false)]
	[DataRow("/", false, true)]
	[DataRow("/Policies/", true, true)]
	[DataRow("/blog/", false, true)]
	[DataRow("/blog/article1", false, true)]
	[DataRow("/blog/categories/", false, true)]
	[DataRow("docs/", false, true)]
	[DataRow("docs/page1", false, true)]
	[DataRow("docs/page2/", false, true)]
	public void Test_Pages_ContainsPageForRoute(
		string route, bool isCaseSensitive, bool expected)
	{
		var actual = _pages.ContainsResourceForRoute(route, isCaseSensitive);
		Assert.AreEqual(expected, actual);
	}

	[DataTestMethod]
	[DataRow("abc/", false, false)]
	[DataRow("/abc", false, false)]
	[DataRow("/abc/", false, false)]
	[DataRow("/abc/xyz", false, false)]
	[DataRow("/", false, true)]
	[DataRow("/Policies/", true, true)]
	[DataRow("/blog/", false, true)]
	[DataRow("/blog/article1", false, true)]
	[DataRow("/blog/categories/", false, true)]
	[DataRow("docs/", false, true)]
	[DataRow("docs/page1", false, true)]
	[DataRow("docs/page2/", false, true)]
	[DataRow("/segment/page/1", false, false)]
	[DataRow("/segment/page/1?p1=v1", false, true)]
	[DataRow("/segment/page/2/", false, false)]
	[DataRow("/segment/page/2/?p1=v1", false, true)]
	public void Test_Pages_ContainsPageForUrl(
		string url, bool isCaseSensitive, bool expected)
	{
		var actual = _pages.ContainsResourceForUrl(url, isCaseSensitive);
		Assert.AreEqual(expected, actual);
	}


	[DataTestMethod]
	[DataRow("abc/", false, false)]
	[DataRow("/abc", false, false)]
	[DataRow("/abc/", false, false)]
	[DataRow("/abc/xyz", false, false)]
	[DataRow("/", false, true)]
	[DataRow("/Policies/", true, true)]
	[DataRow("/blog/", false, true)]
	[DataRow("/blog/article1", false, true)]
	[DataRow("/blog/categories/", false, true)]
	[DataRow("docs/", false, true)]
	[DataRow("docs/page1", false, true)]
	[DataRow("docs/page2/", false, true)]
	public void Test_Pages_GetPageForRoute(
		string route, bool isCaseSensitive, bool expected)
	{
		var page = _pages.GetResourceForRoute(route, isCaseSensitive);
		var actual = page is not null;
		Assert.AreEqual(expected, actual);
	}

	[DataTestMethod]
	[DataRow("abc/", false, false)]
	[DataRow("/abc", false, false)]
	[DataRow("/abc/", false, false)]
	[DataRow("/abc/xyz", false, false)]
	[DataRow("/", false, true)]
	[DataRow("/Policies/", true, true)]
	[DataRow("/blog/", false, true)]
	[DataRow("/blog/article1", false, true)]
	[DataRow("/blog/categories/", false, true)]
	[DataRow("docs/", false, true)]
	[DataRow("docs/page1", false, true)]
	[DataRow("docs/page2/", false, true)]
	[DataRow("/segment/page/1", false, false)]
	[DataRow("/segment/page/1?p1=v1", false, true)]
	[DataRow("/segment/page/2/", false, false)]
	[DataRow("/segment/page/2/?p1=v1", false, true)]
	public void Test_Pages_GetPageForUrl(
		string url, bool isCaseSensitive, bool expected)
	{
		var page = _pages.GetResourceForUrl(url, isCaseSensitive);
		var actual = page is not null;
		Assert.AreEqual(expected, actual);
	}

}
