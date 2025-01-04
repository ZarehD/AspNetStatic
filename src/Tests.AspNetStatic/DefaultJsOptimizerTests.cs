using AspNetStatic.Optimizer;
using ThrowGuard;
using WebMarkupMin.Core;

namespace Tests.AspNetStatic;

[TestClass]
public class DefaultJsOptimizerTests
{
	private static readonly DefaultJsOptimizer _optimizer = new(new CrockfordJsMinifier());

	const string outFilePathname = "subfolder/{0}";
	const string inputContent = "function test() { let some-var = some-value; }";



	[DataTestMethod]
	[DataRow("file.abcd", OptimizationType.Auto)]
	[DataRow("file.abcd", OptimizationType.None)]
	[DataRow("file.abcd", OptimizationType.Js)]
	[DataRow("file.js", OptimizationType.Auto)]
	[DataRow("file.js", OptimizationType.Js)]
	[DataRow("file.js", OptimizationType.Html)]
	public void Returns_Valid_Content(string fileName, OptimizationType optimizationType)
	{
		var res = new JsResource($"/{fileName}")
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
