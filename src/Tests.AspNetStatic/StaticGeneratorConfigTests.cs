using AspNetStatic.Optimizer;
using WebMarkupMin.Core;

namespace Tests.AspNetStatic;

[TestClass]
public class StaticGeneratorConfigTests
{
	private static readonly string _destRoot = "/dest-root";
	private static readonly string _testFileName = "test";
	private static readonly string _testFileExt = ".ext";
	private static readonly string _testIndexFile = _testFileName + _testFileExt;
	private static readonly string[] _testFileExclusions = ["test"];

	private static readonly string _defaultFileName = Consts.DefaultIndexFile;
	private static readonly string _defaultFileExt = Consts.Ext_Htm;
	private static readonly string _defaultIndexFile = Consts.DefaultIndexFileFullName;
	private static readonly string[] _defaultFileExclusions = Consts.DefaultFileExclusions;

	private static readonly PageResource[] _pageArray = [new("/")];
	private static readonly NonPageResource[] _cssFileArray = [new("/file.css")];
	private static readonly NonPageResource[] _jsFileArray = [new("/file.js")];
	private static readonly NonPageResource[] _binFileArray = [new("/file.bin")];

	private static readonly List<PageResource> _pages = new(_pageArray);
	private static readonly List<NonPageResource> _otherResources =
		new(_cssFileArray.Concat(_jsFileArray).Concat(_binFileArray));
	private static readonly List<ResourceInfoBase> _hasPagesNoOther = new(_pageArray);
	private static readonly List<ResourceInfoBase> _noPagesHasOther = new(_otherResources);
	private static readonly List<ResourceInfoBase> _hasPagesHasOther = [];

	private static readonly ICssMinifier _cssMinifier = new KristensenCssMinifier();
	private static readonly IJsMinifier _jsMinifier = new CrockfordJsMinifier();

	private static readonly IMarkupOptimizer _markupOptimizer = new DefaultMarkupOptimizer(null, null, null, _cssMinifier, _jsMinifier);
	private static readonly ICssOptimizer _cssOptimizer = new DefaultCssOptimizer(_cssMinifier);
	private static readonly IJsOptimizer _jsOptimizer = new DefaultJsOptimizer(_jsMinifier);
	private static readonly IBinOptimizer _binOptimizer = new NullBinOptimizer();
	private static readonly DefaultOptimizerSelector _optimizerSelector = new(_markupOptimizer, _cssOptimizer, _jsOptimizer, _binOptimizer);

	//-

	static StaticGeneratorConfigTests()
	{
		_hasPagesHasOther.Clear();
		_hasPagesHasOther.AddRange(_pageArray);
		_hasPagesHasOther.AddRange(_otherResources);
	}

	//-

	[DataTestMethod]
	[DataRow(true, true, true, true)]
	[DataRow(false, false, false, false)]
	[DataRow(false, false, false, true)]
	public void Ctor_v1_Tests(
			bool createDefaultFile,
			bool fixupHrefValues,
			bool enableOptimization,
			bool specifyOptimizerSelector)
	{
		var actual = new StaticGeneratorConfig(
			_hasPagesHasOther, _destRoot,
			createDefaultFile,
			fixupHrefValues,
			enableOptimization,
			specifyOptimizerSelector ? _optimizerSelector : null);

		Assert.IsNotNull(actual.Pages);
		Assert.AreEqual(_pages.Count, actual.Pages.Count());
		Assert.AreEqual(_pages[0], actual.Pages.First());

		Assert.IsNotNull(actual.OtherResources);
		Assert.AreEqual(_otherResources.Count, actual.OtherResources.Count());
		Assert.AreEqual(_otherResources[0], actual.OtherResources.First());

		Assert.AreEqual(_destRoot, actual.DestinationRoot);
		Assert.AreEqual(createDefaultFile, actual.AlwaysCreateDefaultFile);
		Assert.AreEqual(fixupHrefValues, actual.UpdateLinks);
		Assert.AreEqual(_defaultFileName, actual.DefaultFileName);
		Assert.AreEqual(_defaultFileExt, actual.PageFileExtension);
		Assert.AreEqual(_defaultIndexFile, actual.IndexFileName);

		Assert.IsNotNull(actual.DefaultFileExclusions);
		Assert.AreEqual(_defaultFileExclusions.Length, actual.DefaultFileExclusions.Count);

		Assert.AreEqual(enableOptimization, actual.OptimizePageContent);
		Assert.AreEqual(
			specifyOptimizerSelector ? _optimizerSelector : null,
			actual.OptimizerSelector);
	}

	[DataTestMethod]
	[DataRow(true, true, true, true)]
	[DataRow(false, false, false, false)]
	[DataRow(false, false, false, true)]
	public void Ctor_v2_Tests(
		bool createDefaultFile,
		bool fixupHrefValues,
		bool enableOptimization,
		bool specifyOptimizerSelector)
	{
		var actual = new StaticGeneratorConfig(
			_hasPagesHasOther, _destRoot,
			createDefaultFile,
			fixupHrefValues,
			_testFileName,
			_testFileExt,
			_testFileExclusions,
			enableOptimization,
			specifyOptimizerSelector ? _optimizerSelector : null);

		Assert.IsNotNull(actual.Pages);
		Assert.AreEqual(_pages.Count, actual.Pages.Count());
		Assert.AreEqual(_pages[0], actual.Pages.First());

		Assert.IsNotNull(actual.OtherResources);
		Assert.AreEqual(_otherResources.Count, actual.OtherResources.Count());
		Assert.AreEqual(_otherResources[0], actual.OtherResources.First());

		Assert.AreEqual(_destRoot, actual.DestinationRoot);
		Assert.AreEqual(createDefaultFile, actual.AlwaysCreateDefaultFile);
		Assert.AreEqual(fixupHrefValues, actual.UpdateLinks);
		Assert.AreEqual(_testFileName, actual.DefaultFileName);
		Assert.AreEqual(_testFileExt, actual.PageFileExtension);
		Assert.AreEqual(_testIndexFile, actual.IndexFileName);

		Assert.IsNotNull(actual.DefaultFileExclusions);
		Assert.AreEqual(_testFileExclusions.Length, actual.DefaultFileExclusions.Count);

		Assert.AreEqual(enableOptimization, actual.OptimizePageContent);
		Assert.AreEqual(
			specifyOptimizerSelector ? _optimizerSelector : null,
			actual.OptimizerSelector);
	}


	[TestMethod]
	public void Ctor_v1_NoPages_Pages_ShdBe_Empty()
	{
		var actual = new StaticGeneratorConfig(
			_noPagesHasOther, _destRoot,
			false, false, false, null);

		Assert.IsNotNull(actual.Pages);
		Assert.AreEqual(0, actual.Pages.Count());
	}

	[TestMethod]
	public void Ctor_v1_NoOther_OtherRes_ShdBe_Empty()
	{
		var actual = new StaticGeneratorConfig(
			_hasPagesNoOther, _destRoot,
			false, false, false, null);

		Assert.IsNotNull(actual.OtherResources);
		Assert.AreEqual(0, actual.OtherResources.Count());
	}

	[TestMethod]
	public void Ctor_v2_NoPages_Pages_ShdBe_Empty()
	{
		var actual = new StaticGeneratorConfig(
			_noPagesHasOther, _destRoot,
			false, false,
			_testFileName, _testFileExt, _testFileExclusions,
			false, null);

		Assert.IsNotNull(actual.Pages);
		Assert.AreEqual(0, actual.Pages.Count());
	}

	[TestMethod]
	public void Ctor_v2_NoOther_OtherRes_ShdBe_Empty()
	{
		var actual = new StaticGeneratorConfig(
			_hasPagesNoOther, _destRoot,
			false, false,
			_testFileName, _testFileExt, _testFileExclusions,
			false, null);

		Assert.IsNotNull(actual.OtherResources);
		Assert.AreEqual(0, actual.OtherResources.Count());
	}

	//--

	[TestMethod]
	public void Ctor_v1_Good_DefaultFile_NameAndExt()
	{
		var actual = new StaticGeneratorConfig(
			_pages, _destRoot, true, true,
			_testFileName, _testFileExt,
			[]);

		Assert.AreEqual(_testFileName, actual.DefaultFileName, true);
		Assert.AreEqual(_testFileExt, actual.PageFileExtension, true);
	}

	[TestMethod]
	public void Ctor_v1_Good_DefaultFile_Exclusions()
	{
		var actual = new StaticGeneratorConfig(
			_pages, _destRoot, true, true,
			_testFileName, _testFileExt,
			_testFileExclusions);

		Assert.IsNotNull(actual.DefaultFileExclusions);
		Assert.AreNotEqual(0, actual.DefaultFileExclusions.Count);

		Assert.AreEqual(
			_testFileExclusions.Length,
			actual.DefaultFileExclusions.Count);

		Assert.AreEqual(
			_testFileExclusions[0],
			actual.DefaultFileExclusions.First(),
			true);
	}

	//--

	[TestMethod]
	public void Ctor_v1_Null_DefaultFileExt_Shd_Throw()
	{
		Assert.ThrowsException<ArgumentNullException>(
			() => new StaticGeneratorConfig(
				_pages, _destRoot, true, true,
				null!, _testFileExt,
				[]));
	}

	[TestMethod]
	public void Ctor_v1_Empty_DefaultFileExt_Shd_Throw()
	{
		Assert.ThrowsException<ArgumentException>(
			() => new StaticGeneratorConfig(
				_pages, _destRoot, true, true,
				string.Empty, _testFileExt,
				[]));
	}

	[TestMethod]
	public void Ctor_v1_Whitespace_DefaultFileExt_Shd_Throw()
	{
		Assert.ThrowsException<ArgumentException>(
			() => new StaticGeneratorConfig(
				_pages, _destRoot, true, true,
				" ", _testFileExt,
				[]));
	}

	//--

	[TestMethod]
	public void Ctor_v1_Null_DefaultFileName_Shd_Throw()
	{
		Assert.ThrowsException<ArgumentNullException>(
			() => new StaticGeneratorConfig(
				_pages, _destRoot, true, true,
				_testFileName, null!,
				[]));
	}

	[TestMethod]
	public void Ctor_v1_Empty_DefaultFileName_Shd_Throw()
	{
		Assert.ThrowsException<ArgumentException>(
			() => new StaticGeneratorConfig(
				_pages, _destRoot, true, true,
				_testFileName, string.Empty,
				[]));
	}

	[TestMethod]
	public void Ctor_v1_Whitespace_DefaultFileName_Shd_Throw()
	{
		Assert.ThrowsException<ArgumentException>(
			() => new StaticGeneratorConfig(
				_pages, _destRoot, true, true,
				_testFileName, " ",
				[]));
	}

	//--

	[TestMethod]
	public void Ctor_v1_Null_DestRoot_Shd_Throw()
	{
		Assert.ThrowsException<ArgumentNullException>(
			() => new StaticGeneratorConfig(
				_pages, null!, true, true,
				true, null));
	}

	[TestMethod]
	public void Ctor_v1_Empty_DestRoot_Shd_Throw()
	{
		Assert.ThrowsException<ArgumentException>(
			() => new StaticGeneratorConfig(
				_pages, string.Empty, true, true,
				true, null));
	}

	[TestMethod]
	public void Ctor_v1_Whitespace_DestRoot_Shd_Throw()
	{
		Assert.ThrowsException<ArgumentException>(
			() => new StaticGeneratorConfig(
				_pages, " ", true, true,
				true, null));
	}

	//--

	[TestMethod]
	public void Ctor_v1_Optimize_NoOptimizer_Shd_Throw()
	{
		Assert.ThrowsException<ArgumentNullException>(
			() => new StaticGeneratorConfig(
				null, _destRoot, true, true,
				true, null));
	}

	[TestMethod]
	public void Ctor_v2_Optimize_NoOptimizer_Shd_Throw()
	{
		Assert.ThrowsException<ArgumentNullException>(
			() => new StaticGeneratorConfig(
				null, _destRoot, true, true,
				_testFileName, _testFileExt, _testFileExclusions,
				true, null));
	}
}
