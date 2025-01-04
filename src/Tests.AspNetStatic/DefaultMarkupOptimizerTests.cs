using AspNetStatic.Optimizer;
using ThrowGuard;
using WebMarkupMin.Core;

namespace Tests.AspNetStatic;

[TestClass]
public class DefaultMarkupOptimizerTests
{
	private static readonly DefaultMarkupOptimizer _optimizer =
		new(null, null, null,
			new KristensenCssMinifier(),
			new CrockfordJsMinifier());

	const string outFilePathname = "subfolder/{0}";
	const string inputContent = "<html><head><title>title</title></head><body>html content</body></html>";



	[DataTestMethod]
	[DataRow("file.abcd", OptimizationType.Auto)]
	[DataRow("file.abcd", OptimizationType.None)]
	[DataRow("file.htm", OptimizationType.Auto)]
	[DataRow("file.htm", OptimizationType.Html)]
	[DataRow("file.html", OptimizationType.Auto)]
	[DataRow("file.html", OptimizationType.Html)]
	[DataRow("file.xhtm", OptimizationType.Auto)]
	[DataRow("file.xhtm", OptimizationType.Xhtml)]
	[DataRow("file.xhtml", OptimizationType.Auto)]
	[DataRow("file.xhtml", OptimizationType.Xhtml)]
	[DataRow("file.xml", OptimizationType.Auto)]
	[DataRow("file.xml", OptimizationType.Xml)]
	public void Returns_Valid_Content(string fileName, OptimizationType optimizationType)
	{
		var res = new PageResource($"/{fileName}")
		{
			OutFile = outFilePathname.SF(fileName),
			OptimizationType = optimizationType
		};

		var result = _optimizer.Execute(inputContent, res, outFilePathname);

		Assert.IsNotNull(result);
		Assert.IsFalse(result.HasErrors);
		Assert.IsFalse(result.HasWarnings);
		Assert.AreEqual(0, result.Errors.Count);
		Assert.AreEqual(0, result.Warnings.Count);
	}
}
