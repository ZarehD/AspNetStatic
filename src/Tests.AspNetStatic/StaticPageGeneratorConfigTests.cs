using WebMarkupMin.Core;

namespace Tests.AspNetStatic
{
	[TestClass]
	public class StaticPageGeneratorConfigTests
	{
		private static readonly string _destRoot = "/dest-root";
		private static readonly string _defaultFileName = "test";
		private static readonly string _defaultFileExt = ".ext";
		private static readonly string[] __defaultFileExclusions = { "test" };

		private static readonly List<PageInfo> _pages =
			new(new PageInfo[]
			{
				new("/"),
			});

		private static readonly HtmlMinifier _htmlMinifier = new();
		private static readonly XhtmlMinifier _xhtmlMinifier = new();
		private static readonly XmlMinifier _xmlMinifier = new();

		private static readonly OptimizerSelector _minifierChooser =
			new(_htmlMinifier, _xhtmlMinifier, _xmlMinifier);




		[DataTestMethod]
		[DataRow(true, true, true, true)]
		[DataRow(false, false, false, false)]
		[DataRow(false, false, false, true)]
		public void Test_Ctor_Good(
			bool createDefaultFile,
			bool fixupHrefValues,
			bool enableOptimization,
			bool specifyOptimizerSelector)
		{
			var actual = new StaticPageGeneratorConfig(
				_pages, _destRoot,
				createDefaultFile,
				fixupHrefValues,
				enableOptimization,
				specifyOptimizerSelector ? _minifierChooser : null);

			Assert.IsNotNull(actual.Pages);
			Assert.IsTrue(actual.Pages.Any());
			Assert.AreEqual(_pages.Count, actual.Pages.Count);
			Assert.AreEqual(_pages[0], actual.Pages.First());

			Assert.AreEqual(_destRoot, actual.DestinationRoot, true);
			Assert.AreEqual(createDefaultFile, actual.AlwaysCreateDefaultFile);
			Assert.AreEqual(fixupHrefValues, actual.UpdateLinks);
			Assert.AreEqual("index", actual.DefaultFileName, true);
			Assert.AreEqual(".html", actual.PageFileExtension, true);
			Assert.AreEqual("index.html", actual.IndexFileName, true);
			Assert.IsNotNull(actual.DefaultFileExclusions);
			Assert.IsTrue(actual.DefaultFileExclusions.Any());
			Assert.AreEqual(enableOptimization, actual.OptimizePageContent);
			Assert.AreEqual(
				specifyOptimizerSelector ? _minifierChooser : null,
				actual.OptimizerSelector);
		}

		[TestMethod]
		public void Test_Ctor_Good_DefaultFile_NameAndExt()
		{
			var actual = new StaticPageGeneratorConfig(
				_pages, _destRoot, true, true,
				_defaultFileName, _defaultFileExt,
				Enumerable.Empty<string>());

			Assert.AreEqual(_defaultFileName, actual.DefaultFileName, true);
			Assert.AreEqual(_defaultFileExt, actual.PageFileExtension, true);
		}

		[TestMethod]
		public void Test_Ctor_Good_DefaultFile_Exclusions()
		{
			var actual = new StaticPageGeneratorConfig(
				_pages, _destRoot, true, true,
				_defaultFileName, _defaultFileExt,
				__defaultFileExclusions);

			Assert.IsNotNull(actual.DefaultFileExclusions);
			Assert.IsTrue(actual.DefaultFileExclusions.Any());

			Assert.AreEqual(
				__defaultFileExclusions.Length,
				actual.DefaultFileExclusions.Count);

			Assert.AreEqual(
				__defaultFileExclusions[0],
				actual.DefaultFileExclusions.First(),
				true);
		}


		[TestMethod]
		public void Test_Ctor_Bad_DefaultFile_Ext()
		{
			Assert.ThrowsException<ArgumentNullException>(
				() => new StaticPageGeneratorConfig(
					_pages, _destRoot, true, true,
					null!, _defaultFileExt,
					Enumerable.Empty<string>()));

			Assert.ThrowsException<ArgumentException>(
				() => new StaticPageGeneratorConfig(
					_pages, _destRoot, true, true,
					string.Empty, _defaultFileExt,
					Enumerable.Empty<string>()));

			Assert.ThrowsException<ArgumentException>(
				() => new StaticPageGeneratorConfig(
					_pages, _destRoot, true, true,
					" ", _defaultFileExt,
					Enumerable.Empty<string>()));
		}

		[TestMethod]
		public void Test_Ctor_Bad_DefaultFile_Name()
		{
			Assert.ThrowsException<ArgumentNullException>(
				() => new StaticPageGeneratorConfig(
					_pages, _destRoot, true, true,
					_defaultFileName, null!,
					Enumerable.Empty<string>()));

			Assert.ThrowsException<ArgumentException>(
				() => new StaticPageGeneratorConfig(
					_pages, _destRoot, true, true,
					_defaultFileName, string.Empty,
					Enumerable.Empty<string>()));

			Assert.ThrowsException<ArgumentException>(
				() => new StaticPageGeneratorConfig(
					_pages, _destRoot, true, true,
					_defaultFileName, " ",
					Enumerable.Empty<string>()));
		}

		[TestMethod]
		public void Test_Ctor_Bad_DestRoot()
		{
			Assert.ThrowsException<ArgumentNullException>(
				() => new StaticPageGeneratorConfig(
					_pages, null!, true, true,
					true, null));

			Assert.ThrowsException<ArgumentException>(
				() => new StaticPageGeneratorConfig(
					_pages, string.Empty, true, true,
					true, null));

			Assert.ThrowsException<ArgumentException>(
				() => new StaticPageGeneratorConfig(
					_pages, " ", true, true,
					true, null));
		}

		[TestMethod]
		public void Test_Ctor_Bad_Optimize_NoOptimizer()
		{
			Assert.ThrowsException<ArgumentNullException>(
				() => new StaticPageGeneratorConfig(
					_pages, _destRoot, true, true,
					true, null));
		}
	}
}
