namespace Tests.AspNetStatic;

[TestClass]
public class LinkHelperTests
{
	private const string File_Index_p1v1 = @"index-p1v1.htm";
	private const string File_Home_P1 = @"home\page1.htm";
	private const string File_Pages_Page_Rp2 = @"pages\page-rp2.htm";
	private const string File_Pages_Page_Rp2_P1V1 = @"pages\page-rp2-p1v1.htm";

	private const string Url_Index_p1v1 = "index-p1v1.htm";
	private const string Url_Home_P1 = "home/page1.htm";
	private const string Url_Pages_Page_Rp2 = "pages/page-rp2.htm";
	private const string Url_Pages_Page_Rp2_P1V1 = "pages/page-rp2-p1v1.htm";

	private static readonly string _indexFileName = "index.html";
	private static readonly string _fileExtension = ".html";
	private static readonly string[] _exclusions = ["index", "default"];
	private static readonly List<PageResource> _pages =
		new(
		[
			new("/"),
			new("/") { Query = "?p1=v1", OutFile = File_Index_p1v1 },
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
			new("/home/p1"){ OutFile = File_Home_P1 },
			new("/pages/page") { Query = "?p1=v1" },
			new("/pages/page/") { Query = "?p1=v1" },
			new("/pages/page/rp1"),
			new("/pages/page/rp2") { OutFile = File_Pages_Page_Rp2 },
			new("/pages/page/rp2") { Query = "?p1=v1", OutFile = File_Pages_Page_Rp2_P1V1 },
		]);


	#region Fixup Href Values...

	[DataTestMethod]
	[DataRow(@"<a class=""abcd"" href=""/"" title=""test"">", @"<a class=""abcd"" href=""/index.html"" title=""test"">")]
	[DataRow(@"<a class=""abcd"" href=""/docs"" title=""test"">", @"<a class=""abcd"" href=""/docs/index.html"" title=""test"">")]
	[DataRow(@"<a href="""" title=""test"">", @"<a href="""" title=""test"">")]
	[DataRow(@"<a href=""/"" title=""test"">", @"<a href=""/index.html"" title=""test"">")]
	[DataRow(
		@"<a href=""/docs"" title=""test""> <div>abcd</div> <a href=""/docs"" title=""test"">",
		@"<a href=""/docs/index.html"" title=""test""> <div>abcd</div> <a href=""/docs/index.html"" title=""test"">")]
	[DataRow(@"<a href=""/docs/"" title=""test"">", @"<a href=""/docs/index.html"" title=""test"">")]
	[DataRow(@"<a href=""/index"" title=""test"">", @"<a href=""/index"" title=""test"">")]
	[DataRow(@"<a href=""/segment/index"" title=""test"">", @"<a href=""/segment/index"" title=""test"">")]
	[DataRow(@"<a href=""segment/index"" title=""test"">", @"<a href=""segment/index"" title=""test"">")]
	[DataRow(@"<a href=""/blogs/blog/article2"">", @"<a href=""/blogs/blog/article2.html"">")]
	[DataRow(@"<a href=""/blogs/blog/categories/"">", @"<a href=""/blogs/blog/categories/index.html"">")]
	[DataRow(@"<a title=""blogs""
   href=""/blog/"">", @"<a title=""blogs""
   href=""/blog/index.html"">")]
	[DataRow(@"<a title=""blogs""
   href=""/blog/article1"">", @"<a title=""blogs""
   href=""/blog/article1.html"">")]
	[DataRow(@"<a href=""/home/p1"">", $@"<a href=""/{Url_Home_P1}"">")]
	[DataRow(@"<a href=""/home/p1/"">", $@"<a href=""/{Url_Home_P1}"">")]
	[DataRow(@"<a href=""home/p1/"">", $@"<a href=""{Url_Home_P1}"">")]
	//[DataRow(@"<a href=""pages/page"">", @"<a href=""pages/page.htm"">")]
	[DataRow(@"<a href=""pages/page?p1=v1"">", @"<a href=""pages/page.html"">")]
	[DataRow(@"<a href=""pages/page/rp1"">", @"<a href=""pages/page/rp1.html"">")]
	[DataRow(@"<a href=""pages/page/?p1=v1"">", @"<a href=""pages/page/index.html"">")]
	[DataRow(@"<a href=""/pages/page/rp2"">", $@"<a href=""/{Url_Pages_Page_Rp2}"">")]
	[DataRow(@"<a href=""/pages/page/rp2?p1=v1"">", $@"<a href=""/{Url_Pages_Page_Rp2_P1V1}"">")]
	//[DataRow(@"<a href=""pages/page"">", @"<a href=""pages/page.htm"">")]
	[DataRow(@"<area href=""/index.html""   alt=""test"">", @"<area href=""/index.html""   alt=""test"">")]
	[DataRow(@"<area href=""/index""   alt=""test"">", @"<area href=""/index""   alt=""test"">")]
	[DataRow(@"<area href=""docs/"" alt=""test"">", @"<area href=""docs/index.html"" alt=""test"">")]
	[DataRow(@"<base href=""/docs/"">", @"<base href=""/docs/"">")]
	[DataRow(@"<link rel=""abcd"" type=""efgh"" href=""/test"">", @"<link rel=""abcd"" type=""efgh"" href=""/test"">")]
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
		var inputFilePath = @".\Data\input.txt"
			.Replace(Consts.BakSlash, Path.DirectorySeparatorChar)
			.Replace(Consts.FwdSlash, Path.DirectorySeparatorChar)
			;

		var outputFileName = @".\Data\output_html.txt"
			.Replace(Consts.BakSlash, Path.DirectorySeparatorChar)
			.Replace(Consts.FwdSlash, Path.DirectorySeparatorChar)
			;

		var source = File.ReadAllText(inputFilePath);
		var expected = File.ReadAllText(outputFileName);

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
		var inputFilePath = @".\Data\input.txt"
			.Replace(Consts.BakSlash, Path.DirectorySeparatorChar)
			.Replace(Consts.FwdSlash, Path.DirectorySeparatorChar)
			;

		var outputFileName = @".\Data\output_default.txt"
			.Replace(Consts.BakSlash, Path.DirectorySeparatorChar)
			.Replace(Consts.FwdSlash, Path.DirectorySeparatorChar)
			;

		var source = File.ReadAllText(inputFilePath);
		var expected = File.ReadAllText(outputFileName);

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

	#region Find Page - OBSOLETE

	//[DataTestMethod]
	//[DataRow(@"/pages/page", null)]
	//[DataRow(@"/pages/page/", null)]
	//[DataRow(@"/pages/page", "?p1=v1")]
	//[DataRow(@"/pages/page/", "?p1=v1")]
	//[DataRow(@"/pages/page", "p1=v1")]
	//[DataRow(@"/pages/page/", "p1=v1")]
	//[DataRow(@"/pages/page/route-parm", null)]
	//[DataRow(@"/pages/page/route-parm", "?p1=v1")]
	//public void Test_FindPage(string route, string? query)
	//{
	//	var page = new PageResourcesInfo(route) { QueryString = query };
	//	var pages = new List<PageResourcesInfo>(new[] { page });

	//	var expected = page;
	//	var actual = pages.FindPage(page.Url);

	//	Assert.AreEqual(expected, actual);
	//}

	#endregion
}
