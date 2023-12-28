using WebMarkupMin.Core;

namespace Tests.AspNetStatic
{
	[TestClass]
	public class OptimizerSelectorTests
	{
		private static readonly ICssMinifier _nullCssMinifier = new NullCssMinifier();
		private static readonly IJsMinifier _nullJsMinifier = new NullJsMinifier();

		private static readonly HtmlMinifier _htmlMinifier = new();
		private static readonly XhtmlMinifier _xhtmlMinifier = new();
		private static readonly XmlMinifier _xmlMinifier = new();
		private static readonly ICssMinifier _cssMinifier = new KristensenCssMinifier();
		private static readonly IJsMinifier _jsMinifier = new CrockfordJsMinifier();
		private static readonly IBinOptimizer? _binOptimizer = new TestBinOptimizer();

		private static readonly OptimizerSelector _minifierChooser =
			new(_htmlMinifier, _xhtmlMinifier, _xmlMinifier, _cssMinifier, _jsMinifier, _binOptimizer);

		//--

		private class TestBinOptimizer : IBinOptimizer
		{
			public BinOptimizerResult Execute(byte[] content, BinResource resource, string outFilePathname) => new(content);
		}

		//--

		[TestMethod]
		public void Test_Ctor_Good()
		{
			var actual = new OptimizerSelector(_htmlMinifier, _xhtmlMinifier, _xmlMinifier, _cssMinifier, _jsMinifier, _binOptimizer);
			Assert.IsNotNull(actual);
		}

		[TestMethod]
		public void Test_Ctor_Bad()
		{
			Assert.ThrowsException<ArgumentNullException>(() => new OptimizerSelector(null!, _xhtmlMinifier, _xmlMinifier, _cssMinifier, _jsMinifier, _binOptimizer));
			Assert.ThrowsException<ArgumentNullException>(() => new OptimizerSelector(_htmlMinifier, null!, _xmlMinifier, _cssMinifier, _jsMinifier, _binOptimizer));
			Assert.ThrowsException<ArgumentNullException>(() => new OptimizerSelector(_htmlMinifier, _xhtmlMinifier, null!, _cssMinifier, _jsMinifier, _binOptimizer));
			Assert.ThrowsException<ArgumentNullException>(() => new OptimizerSelector(_htmlMinifier, _xhtmlMinifier, _xmlMinifier, null!, _jsMinifier, _binOptimizer));
			Assert.ThrowsException<ArgumentNullException>(() => new OptimizerSelector(_htmlMinifier, _xhtmlMinifier, _xmlMinifier, _cssMinifier, null!, _binOptimizer));
		}

		//--

		[DataTestMethod]
		[DataRow("file.htm", OptimizerType.Html)]
		[DataRow("file.html", OptimizerType.Html)]
		[DataRow("file.xhtm", OptimizerType.Xhtml)]
		[DataRow("file.xhtml", OptimizerType.Xhtml)]
		[DataRow("file.xml", OptimizerType.Xml)]
		public void Test_Select_MarkupMinifier_Auto(
			string outFile, OptimizerType expectedOptimizerType)
		{
			var page =
				new PageResource("/")
				{
					OutFile = outFile
				};

			Assert.AreEqual(OptimizerType.Auto, page.OptimizerType);

			IMarkupMinifier expected =
				expectedOptimizerType switch
				{
					OptimizerType.Html => _htmlMinifier,
					OptimizerType.Xhtml => _xhtmlMinifier,
					OptimizerType.Xml => _xmlMinifier,
					_ => _htmlMinifier
				};

			var actual = _minifierChooser.SelectFor(page, outFile);

			Assert.AreSame(expected, actual);
		}

		[DataTestMethod]
		[DataRow("file.htm", OptimizerType.Html)]
		[DataRow("file.html", OptimizerType.Html)]
		[DataRow("file.xhtm", OptimizerType.Xhtml)]
		[DataRow("file.xhtml", OptimizerType.Xhtml)]
		[DataRow("file.xml", OptimizerType.Xml)]
		public void Test_Select_MarkupMinifier_Specific(
			string outFile, OptimizerType expectedOptimizerType)
		{
			var page =
				new PageResource("/")
				{
					OutFile = outFile,
					OptimizerType = expectedOptimizerType
				};

			Assert.AreEqual(expectedOptimizerType, page.OptimizerType);

			IMarkupMinifier expected =
				expectedOptimizerType switch
				{
					OptimizerType.Html => _htmlMinifier,
					OptimizerType.Xhtml => _xhtmlMinifier,
					OptimizerType.Xml => _xmlMinifier,
					_ => _htmlMinifier
				};

			var actual = _minifierChooser.SelectFor(page, outFile);

			Assert.AreSame(expected, actual);
		}

		//--

		[DataTestMethod]
		[DataRow("file.css", OptimizerType.Css)]
		public void Test_Select_CssMinifier_Auto(
			string outFile, OptimizerType expectedOptimizerType)
		{
			var resource =
				new CssResource(outFile)
				{
					OutFile = outFile
				};

			Assert.AreEqual(OptimizerType.Auto, resource.OptimizerType);

			ICssMinifier expected =
				expectedOptimizerType switch
				{
					OptimizerType.Css => _cssMinifier,
					_ => _nullCssMinifier
				};

			var actual = _minifierChooser.SelectFor(resource, outFile);

			Assert.AreSame(expected, actual);
		}

		[DataTestMethod]
		[DataRow("file.css", OptimizerType.Css)]
		public void Test_Select_CssMinifier_Specific(
			string outFile, OptimizerType expectedOptimizerType)
		{
			var resource =
				new CssResource(outFile)
				{
					OutFile = outFile,
					OptimizerType = expectedOptimizerType
				};

			Assert.AreEqual(expectedOptimizerType, resource.OptimizerType);

			ICssMinifier expected =
				expectedOptimizerType switch
				{
					OptimizerType.Css => _cssMinifier,
					_ => _nullCssMinifier
				};

			var actual = _minifierChooser.SelectFor(resource, outFile);

			Assert.AreSame(expected, actual);
		}

		//--

		[DataTestMethod]
		[DataRow("file.js", OptimizerType.Js)]
		public void Test_Select_JsMinifier_Auto(
			string outFile, OptimizerType expectedOptimizerType)
		{
			var resource =
				new JsResource(outFile)
				{
					OutFile = outFile
				};

			Assert.AreEqual(OptimizerType.Auto, resource.OptimizerType);

			IJsMinifier expected =
				expectedOptimizerType switch
				{
					OptimizerType.Js => _jsMinifier,
					_ => _nullJsMinifier
				};

			var actual = _minifierChooser.SelectFor(resource, outFile);

			Assert.AreSame(expected, actual);
		}

		[DataTestMethod]
		[DataRow("file.js", OptimizerType.Js)]
		public void Test_Select_JsMinifier_Specific(
			string outFile, OptimizerType expectedOptimizerType)
		{
			var resource =
				new JsResource(outFile)
				{
					OutFile = outFile,
					OptimizerType = expectedOptimizerType
				};

			Assert.AreEqual(expectedOptimizerType, resource.OptimizerType);

			IJsMinifier expected =
				expectedOptimizerType switch
				{
					OptimizerType.Js => _jsMinifier,
					_ => _nullJsMinifier
				};

			var actual = _minifierChooser.SelectFor(resource, outFile);

			Assert.AreSame(expected, actual);
		}

		//--

		[DataTestMethod]
		[DataRow("file.img", OptimizerType.Bin)]
		[DataRow("file.dll", OptimizerType.None)]
		public void Test_Select_JsMinifier(
			string outFile, OptimizerType expectedOptimizerType)
		{
			var resource =
				new BinResource(outFile)
				{
					OutFile = outFile,
					OptimizerType = expectedOptimizerType
				};

			Assert.AreEqual(expectedOptimizerType, resource.OptimizerType);

			IBinOptimizer? expected =
				expectedOptimizerType switch
				{
					OptimizerType.Bin => _binOptimizer,
					_ => null
				};

			var actual = _minifierChooser.SelectFor(resource, outFile);

			Assert.AreSame(expected, actual);
		}
	}
}
