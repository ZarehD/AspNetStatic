namespace Tests.AspNetStatic
{
	[TestClass]
	public class PageInfoExtensionsTests
	{
		private static readonly List<PageInfo> _pages =
			new(new PageInfo[]
			{
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
			});


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
		public void Test_Pages_ContainsRoute(
			string route, bool isCaseSensitive, bool expected)
		{
			var actual = _pages.ContainsPageForRoute(route, isCaseSensitive);
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
		public void Test_Pages_GetForRoute(
			string route, bool isCaseSensitive, bool expected)
		{
			var page = _pages.GetForRoute(route, isCaseSensitive);
			var actual = page is not null;
			Assert.AreEqual(expected, actual);
		}

	}
}
