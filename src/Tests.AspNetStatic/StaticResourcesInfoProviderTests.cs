namespace Tests.AspNetStatic;

[TestClass]
public class StaticResourcesInfoProviderTests
{
	private readonly static List<ResourceInfoBase> Test_Pages = new([new PageResource("/segment/page")]);
	private readonly static List<string> Test_Exclusions = new(["index", "default", "who-this"]);
	private readonly static string Test_DefaultFileName = "default-file-name";
	private readonly static string Test_DefaultFileExtension = ".test-extension";

	private static StaticResourcesInfoProvider GetStandardPagesInfoProvider() =>
		new(Test_Pages,
			Test_DefaultFileName,
			Test_DefaultFileExtension,
			Test_Exclusions);

	//--

	[TestMethod]
	public void Ctor_1()
	{
		var sut = new StaticResourcesInfoProvider();

		Assert.AreEqual(Consts.DefaultIndexFile, sut.DefaultFileName, ignoreCase: false);
		Assert.AreEqual(Consts.Ext_Htm, sut.PageFileExtension, ignoreCase: false);

		Assert.AreEqual(Consts.DefaultFileExclusions.Length, sut.DefaultFileExclusions.Length);
		foreach (var exc in Consts.DefaultFileExclusions)
		{
			Assert.IsTrue(sut.DefaultFileExclusions.Contains(exc));
		}

		Assert.AreEqual(0, sut.PageResources.Count());
	}

	[TestMethod]
	public void Ctor_2()
	{
		var sut = GetStandardPagesInfoProvider();

		Assert.AreEqual(Test_DefaultFileName, sut.DefaultFileName, ignoreCase: false);
		Assert.AreEqual(Test_DefaultFileExtension, sut.PageFileExtension, ignoreCase: false);

		Assert.AreEqual(Test_Exclusions.Count, sut.DefaultFileExclusions.Length);
		foreach (var exc in Test_Exclusions)
		{
			Assert.IsTrue(sut.DefaultFileExclusions.Contains(exc));
		}

		Assert.AreEqual(Test_Pages.Count, sut.PageResources.Count());
		foreach (var url in Test_Pages.Select(p => p.Route))
		{
			Assert.IsTrue(sut.PageResources.Any(p => p.Route.Equals(url)));
		}
	}

	//--

	[TestMethod]
	public void SetDefaultFileName_GoodValue_Should_Succeed()
	{
		var sut = GetStandardPagesInfoProvider();
		var expected = "test";

		sut.SetDefaultFileName(expected);

		Assert.AreEqual(expected, sut.DefaultFileName);
	}

	[TestMethod]
	public void SetDefaultFileName_BadValue_Should_Throw()
	{
		var sut = GetStandardPagesInfoProvider();

		Assert.ThrowsException<ArgumentNullException>(() => sut.SetDefaultFileName(null!));
		Assert.ThrowsException<ArgumentException>(() => sut.SetDefaultFileName(string.Empty));
		Assert.ThrowsException<ArgumentException>(() => sut.SetDefaultFileName(" "));
	}

	//--

	[TestMethod]
	public void SetDefaultFileExtension_GoodValue_Should_Succeed()
	{
		var sut = GetStandardPagesInfoProvider();
		var expected = "test";

		sut.SetDefaultFileExtension(expected);

		Assert.AreEqual(expected, sut.PageFileExtension);
	}

	[TestMethod]
	public void SetDefaultFileExtension_BadValue_Should_Throw()
	{
		var sut = GetStandardPagesInfoProvider();

		Assert.ThrowsException<ArgumentNullException>(() => sut.SetDefaultFileExtension(null!));
		Assert.ThrowsException<ArgumentException>(() => sut.SetDefaultFileExtension(string.Empty));
		Assert.ThrowsException<ArgumentException>(() => sut.SetDefaultFileExtension(" "));
	}

	//--

	[TestMethod]
	public void SetDefaultFileExclusions_GoodValue_Should_Succeed()
	{
		var sut = GetStandardPagesInfoProvider();
		var expected = new[] { "test1", "test2" };

		sut.SetDefaultFileExclusions(expected);

		Assert.AreEqual(expected.Length, sut.DefaultFileExclusions.Length);
		foreach (var x in expected)
		{
			Assert.IsTrue(
				sut.DefaultFileExclusions.Any(
					s => s.Equals(x, StringComparison.InvariantCultureIgnoreCase)));
		}
	}

	[TestMethod]
	public void SetDefaultFileExclusions_BadValue_Should_Throw()
	{
		var sut = GetStandardPagesInfoProvider();

		Assert.ThrowsException<ArgumentException>(() => sut.SetDefaultFileExclusions([null!]));
		Assert.ThrowsException<ArgumentException>(() => sut.SetDefaultFileExclusions([string.Empty]));
		Assert.ThrowsException<ArgumentException>(() => sut.SetDefaultFileExclusions([" "]));
	}

	//--

	[TestMethod]
	public void Add_Page_GoodValue_Should_Succeed()
	{
		var sut = new StaticResourcesInfoProvider();
		var expected = "route";
		var res = new PageResource(expected);

		Assert.AreEqual(expected, res.Route);

		var actual = sut.Add(res);

		Assert.IsNotNull(actual);
		Assert.AreSame(sut, actual);
		Assert.AreNotEqual(0, actual.Resources.Count());
		Assert.AreEqual(expected, actual.Resources.First().Route);
	}

	[TestMethod]
	public void Add_Page_BadValue_Should_Throw()
	{
		var sut = new StaticResourcesInfoProvider();

		var r1Route = "route-1";
		var r2Route = "route-2";
		var res1 = new PageResource(r1Route);
		var res2 = new PageResource(r2Route);

		var expected = new List<PageResource>() { res1, res2 };

		var actual = sut.Add(expected);

		Assert.IsNotNull(actual);
		Assert.AreSame(sut, actual);
		Assert.AreEqual(expected.Count, actual.Resources.Count());
		Assert.AreEqual(expected.First().Route, actual.Resources.First().Route);
		Assert.AreEqual(expected.Last().Route, actual.Resources.Last().Route);
	}

	//--

	[TestMethod]
	public void Add_Css_GoodValue_Should_Succeed()
	{
		var sut = new StaticResourcesInfoProvider();
		var expected = "route";
		var res = new CssResource(expected);

		Assert.AreEqual(expected, res.Route);

		var actual = sut.Add(res);

		Assert.IsNotNull(actual);
		Assert.AreSame(sut, actual);
		Assert.AreNotEqual(0, actual.Resources.Count());
		Assert.AreEqual(expected, actual.Resources.First().Route);
	}

	[TestMethod]
	public void Add_Css_BadValue_Should_Throw()
	{
		var sut = new StaticResourcesInfoProvider();

		var r1Route = "route-1";
		var r2Route = "route-2";
		var res1 = new CssResource(r1Route);
		var res2 = new CssResource(r2Route);

		var expected = new List<CssResource>() { res1, res2 };

		var actual = sut.Add(expected);

		Assert.IsNotNull(actual);
		Assert.AreSame(sut, actual);
		Assert.AreEqual(expected.Count, actual.Resources.Count());
		Assert.AreEqual(expected.First().Route, actual.Resources.First().Route);
		Assert.AreEqual(expected.Last().Route, actual.Resources.Last().Route);
	}

	//--

	[TestMethod]
	public void Add_Js_GoodValue_Should_Succeed()
	{
		var sut = new StaticResourcesInfoProvider();
		var expected = "route";
		var res = new JsResource(expected);

		Assert.AreEqual(expected, res.Route);

		var actual = sut.Add(res);

		Assert.IsNotNull(actual);
		Assert.AreSame(sut, actual);
		Assert.AreNotEqual(0, actual.Resources.Count());
		Assert.AreEqual(expected, actual.Resources.First().Route);
	}

	[TestMethod]
	public void Add_Js_BadValue_Should_Throw()
	{
		var sut = new StaticResourcesInfoProvider();

		var r1Route = "route-1";
		var r2Route = "route-2";
		var res1 = new JsResource(r1Route);
		var res2 = new JsResource(r2Route);

		var expected = new List<JsResource>() { res1, res2 };

		var actual = sut.Add(expected);

		Assert.IsNotNull(actual);
		Assert.AreSame(sut, actual);
		Assert.AreEqual(expected.Count, actual.Resources.Count());
		Assert.AreEqual(expected.First().Route, actual.Resources.First().Route);
		Assert.AreEqual(expected.Last().Route, actual.Resources.Last().Route);
	}

	//--

	[TestMethod]
	public void Add_Bin_GoodValue_Should_Succeed()
	{
		var sut = new StaticResourcesInfoProvider();
		var expected = "route";
		var res = new BinResource(expected);

		Assert.AreEqual(expected, res.Route);

		var actual = sut.Add(res);

		Assert.IsNotNull(actual);
		Assert.AreSame(sut, actual);
		Assert.AreNotEqual(0, actual.Resources.Count());
		Assert.AreEqual(expected, actual.Resources.First().Route);
	}

	[TestMethod]
	public void Add_Bin_BadValue_Should_Throw()
	{
		var sut = new StaticResourcesInfoProvider();

		var r1Route = "route-1";
		var r2Route = "route-2";
		var res1 = new BinResource(r1Route);
		var res2 = new BinResource(r2Route);

		var expected = new List<BinResource>() { res1, res2 };

		var actual = sut.Add(expected);

		Assert.IsNotNull(actual);
		Assert.AreSame(sut, actual);
		Assert.AreEqual(expected.Count, actual.Resources.Count());
		Assert.AreEqual(expected.First().Route, actual.Resources.First().Route);
		Assert.AreEqual(expected.Last().Route, actual.Resources.Last().Route);
	}

}