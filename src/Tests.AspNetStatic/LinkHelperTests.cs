namespace Tests.AspNetStatic
{
	[TestClass]
	public class LinkHelperTests
	{
		private static readonly string _indexFileName = "index.html";
		private static readonly string _fileExtension = ".html";
		private static readonly string[] _exclusions = new[] { "index", "default" };
		private static readonly List<PageInfo> _pages =
			new(new PageInfo[]
			{
				new("/"),
				new("/privacy"),
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
				new("/home/p1"){ OutFilePathname = @"home\page1.htm" },
				new("/pages/page") { QueryString = "?p1=q1" },
				new("/pages/page") { QueryString = "/rp1" },
				new("/pages/page/") { QueryString = "?p1=q1" },
			});


		#region Fixup Href Values...

		[DataTestMethod]
		[DataRow(@"<a href="""" title=""test"">", @"<a href="""" title=""test"">")]
		[DataRow(@"<a href=""/"" title=""test"">", @"<a href=""/index.html"" title=""test"">")]
		[DataRow(@"<a href=""/docs/"" title=""test"">", @"<a href=""/docs/index.html"" title=""test"">")]
		[DataRow(
			@"<a href=""/docs"" title=""test""> <div>abcd</div> <a href=""/docs"" title=""test"">",
			@"<a href=""/docs/index.html"" title=""test""> <div>abcd</div> <a href=""/docs/index.html"" title=""test"">")]
		[DataRow(@"<a href=""/index"" title=""test"">", @"<a href=""/index"" title=""test"">")]
		[DataRow(@"<a href=""/segment/index"" title=""test"">", @"<a href=""/segment/index"" title=""test"">")]
		[DataRow(@"<a href=""segment/index"" title=""test"">", @"<a href=""segment/index"" title=""test"">")]
		[DataRow(@"<a class=""abcd"" href=""/"" title=""test"">", @"<a class=""abcd"" href=""/index.html"" title=""test"">")]
		[DataRow(@"<a class=""abcd"" href=""/docs"" title=""test"">", @"<a class=""abcd"" href=""/docs/index.html"" title=""test"">")]
		[DataRow(@"<a href=""/blogs/blog/article2"">", @"<a href=""/blogs/blog/article2.html"">")]
		[DataRow(@"<a href=""/blogs/blog/categories/"">", @"<a href=""/blogs/blog/categories/index.html"">")]
		[DataRow(@"<a title=""blogs""
   href=""/blog/"">", @"<a title=""blogs""
   href=""/blog/index.html"">")]
		[DataRow(@"<a title=""blogs""
   href=""/blog/article1"">", @"<a title=""blogs""
   href=""/blog/article1.html"">")]
		[DataRow(@"<area href=""/index.html""   alt=""test"">", @"<area href=""/index.html""   alt=""test"">")]
		[DataRow(@"<area href=""/index""   alt=""test"">", @"<area href=""/index""   alt=""test"">")]
		[DataRow(@"<area href=""docs/"" alt=""test"">", @"<area href=""docs/index.html"" alt=""test"">")]
		[DataRow(@"<base href=""/docs/"">", @"<base href=""/docs/"">")]
		[DataRow(@"<link rel=""abcd"" type=""efgh"" href=""/test"">", @"<link rel=""abcd"" type=""efgh"" href=""/test"">")]
		[DataRow(@"<a href=""/home/p1/"">", @"<a href=""/home/page1.htm"">")]
		[DataRow(@"<a href=""/home/p1"">", @"<a href=""/home/page1.htm"">")]
		[DataRow(@"<a href=""home/p1/"">", @"<a href=""/home/page1.htm"">")]
		//[DataRow(@"", @"")]
		public void Test_FixupHrefValues(string source, string expected)
		{
			var actual = source.FixupHrefValues(
				_pages,
				_indexFileName, _fileExtension,
				alwaysDefaultFile: false);

			Assert.AreEqual(expected, actual, ignoreCase: true);
		}

		[TestMethod]
		public void Test_FixupHrefValues_FileInput_Html()
		{
			var source = File.ReadAllText(".\\data\\input.txt");
			var expected = File.ReadAllText(".\\data\\output_html.txt");

			Assert.IsNotNull(source);
			Assert.IsTrue(source.Length > 0);

			Assert.IsNotNull(expected);
			Assert.IsTrue(expected.Length > 0);

			var actual = source.FixupHrefValues(
				_pages,
				_indexFileName, _fileExtension,
				alwaysDefaultFile: false);

			Assert.AreEqual(expected, actual, ignoreCase: true);
		}

		[TestMethod]
		public void Test_FixupHrefValues_FileInput_DefaultFile()
		{
			var source = File.ReadAllText(".\\data\\input.txt");
			var expected = File.ReadAllText(".\\data\\output_default.txt");

			Assert.IsNotNull(source);
			Assert.IsTrue(source.Length > 0);

			Assert.IsNotNull(expected);
			Assert.IsTrue(expected.Length > 0);

			var actual = source.FixupHrefValues(
				_pages,
				_indexFileName, _fileExtension,
				alwaysDefaultFile: true);

			Assert.AreEqual(expected, actual, ignoreCase: true);
		}

		#endregion

		#region Find Page...

		[DataTestMethod]
		[DataRow(@"/pages/page", null)]
		[DataRow(@"/pages/page/", null)]
		[DataRow(@"/pages/page", "p1=q1")]
		[DataRow(@"/pages/page/", "?q1=p1")]
		[DataRow(@"/pages/page", "/route-parm")]
		[DataRow(@"/pages/page/", "/route-parm")]
		public void Test_FindPage(string route, string? query)
		{
			var page = new PageInfo(route) { QueryString = query };
			var pages = new List<PageInfo>(new[] { page });

			var expected = page;
			var actual = pages.FindPage(page.Url);

			Assert.AreEqual(expected, actual);
		}

		#endregion
	}
}
