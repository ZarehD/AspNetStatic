namespace Tests.AspNetStatic
{
	[TestClass]
	public class PageInfoTests
	{
		[TestMethod]
		public void Test_Ctor_1()
		{
			var route = "/route/";
			var queryString = "?parm=value";
			var outFilePathname = "\\route\\index.html";
			var changeFrequency = ChangeFrequency.Daily;
			var indexPriority = 1.0;
			var lastModified = DateTime.UtcNow;

			var page =
				new PageInfo(route)
				{
					QueryString = queryString,
					OutFilePathname = outFilePathname,
					ChangeFrequency = changeFrequency,
					IndexPriority = indexPriority,
					LastModified = lastModified
				};

			Assert.IsNotNull(page);
			Assert.AreEqual(page.Route, route, ignoreCase: true);
			Assert.AreEqual(page.QueryString, queryString, ignoreCase: true);
			Assert.AreEqual(page.OutFilePathname, outFilePathname, ignoreCase: true);
			Assert.AreEqual(page.ChangeFrequency, changeFrequency);
			Assert.AreEqual(page.IndexPriority, indexPriority);
			Assert.AreEqual(page.LastModified, lastModified);
		}

		[TestMethod]
		public void Test_Ctor_2()
		{
			var route = "/";
			var queryString = "/x/y";
			var pageFilePathname = "\\index.html";
			var changeFrequency = ChangeFrequency.Never;
			var indexPriority = 0;
			var lastModified = DateTime.MinValue;

			var page =
				new PageInfo(route)
				{
					QueryString = queryString,
					OutFilePathname = pageFilePathname,
				};

			Assert.IsNotNull(page);
			Assert.AreEqual(page.Route, route, ignoreCase: true);
			Assert.AreEqual(page.QueryString, queryString, ignoreCase: true);
			Assert.AreEqual(page.OutFilePathname, pageFilePathname, ignoreCase: true);
			Assert.AreEqual(page.ChangeFrequency, changeFrequency);
			Assert.AreEqual(page.IndexPriority, indexPriority);
			Assert.AreEqual(page.LastModified, lastModified);
		}


		[TestMethod]
		public void Test_Serialization_TextJson()
		{
			var expected =
				new PageInfo("/route/")
				{
					QueryString = "?parm=value",
					OutFilePathname = "\\route\\index.html",
					ChangeFrequency = ChangeFrequency.Daily,
					IndexPriority = 1.0,
					LastModified = DateTime.UtcNow
				};

			var json = System.Text.Json.JsonSerializer.Serialize<PageInfo>(expected);
			var actual = System.Text.Json.JsonSerializer.Deserialize<PageInfo>(json);

			Assert.AreEqual(expected.Route, actual?.Route, ignoreCase: true);
			Assert.AreEqual(expected.QueryString, actual?.QueryString, ignoreCase: true);
			Assert.AreEqual(expected.OutFilePathname, actual?.OutFilePathname, ignoreCase: true);
			Assert.AreEqual(expected.ChangeFrequency, actual?.ChangeFrequency);
			Assert.AreEqual(expected.IndexPriority, actual?.IndexPriority);
			Assert.AreEqual(expected.LastModified, actual?.LastModified);
		}
	}
}
