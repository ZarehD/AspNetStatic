using AspNetStatic.Optimizer;
using ThrowGuard;
using WebMarkupMin.Core;

namespace Tests.AspNetStatic;

[TestClass]
public class DefaultCssOptimizerTests
{
	private static readonly DefaultCssOptimizer _optimizer = new(new KristensenCssMinifier());

	const string outFilePathname = "subfolder/{0}";
	const string inputContent = ".class{ some-rule: some-value; }";



	[DataTestMethod]
	[DataRow("file.abcd", OptimizationType.Auto)]
	[DataRow("file.abcd", OptimizationType.None)]
	[DataRow("file.abcd", OptimizationType.Css)]
	[DataRow("file.css", OptimizationType.Auto)]
	[DataRow("file.css", OptimizationType.Css)]
	[DataRow("file.css", OptimizationType.Html)]
	public void Returns_Valid_Content(string fileName, OptimizationType optimizationType)
	{
		var res = new CssResource($"/{fileName}")
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
