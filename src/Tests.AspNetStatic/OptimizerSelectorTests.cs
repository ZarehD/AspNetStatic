using WebMarkupMin.Core;

namespace Tests.AspNetStatic
{
	[TestClass]
	public class OptimizerSelectorTests
	{
		private static readonly HtmlMinifier _htmlMinifier = new();
		private static readonly XhtmlMinifier _xhtmlMinifier = new();
		private static readonly XmlMinifier _xmlMinifier = new();

		private static readonly OptimizerSelector _minifierChooser =
			new(_htmlMinifier, _xhtmlMinifier, _xmlMinifier);


		[TestMethod]
		public void Test_Ctor_Good()
		{
			var actual = new OptimizerSelector(_htmlMinifier, _xhtmlMinifier, _xmlMinifier);
			Assert.IsNotNull(actual);
		}

		[TestMethod]
		public void Test_Ctor_Bad()
		{
			Assert.ThrowsException<ArgumentNullException>(() => new OptimizerSelector(null!, _xhtmlMinifier, _xmlMinifier));
			Assert.ThrowsException<ArgumentNullException>(() => new OptimizerSelector(_htmlMinifier, null!, _xmlMinifier));
			Assert.ThrowsException<ArgumentNullException>(() => new OptimizerSelector(_htmlMinifier, _xhtmlMinifier, null!));
		}


		[DataTestMethod]
		[DataRow("file.htm", OptimizerType.Html)]
		[DataRow("file.html", OptimizerType.Html)]
		[DataRow("file.xhtm", OptimizerType.Xhtml)]
		[DataRow("file.xhtml", OptimizerType.Xhtml)]
		[DataRow("file.xml", OptimizerType.Xml)]
		public void Test_Select_Minifier_Auto(
			string outFile, OptimizerType expectedOptimizerType)
		{
			var page =
				new PageInfo("/")
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
		public void Test_Select_Minifier_Specific(
			string outFile, OptimizerType expectedOptimizerType)
		{
			var page =
				new PageInfo("/")
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
	}
}
