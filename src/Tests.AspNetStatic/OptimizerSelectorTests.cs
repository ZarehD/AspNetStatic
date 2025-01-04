using AspNetStatic.Optimizer;
using WebMarkupMin.Core;

namespace Tests.AspNetStatic;

[TestClass]
public class OptimizerSelectorTests
{
	private static readonly ICssMinifier _cssMinifier = new KristensenCssMinifier();
	private static readonly IJsMinifier _jsMinifier = new CrockfordJsMinifier();

	private static readonly IMarkupOptimizer _markupOptimizer = new DefaultMarkupOptimizer(null, null, null, _cssMinifier, _jsMinifier);
	private static readonly ICssOptimizer _cssOptimizer = new DefaultCssOptimizer(_cssMinifier);
	private static readonly IJsOptimizer _jsOptimizer = new DefaultJsOptimizer(_jsMinifier);
	private static readonly IBinOptimizer _binOptimizer = new NullBinOptimizer();

	private static readonly DefaultOptimizerSelector _optimizerSelector = new(_markupOptimizer, _cssOptimizer, _jsOptimizer, _binOptimizer);

	//-

	[DataTestMethod]
	[DataRow(OptimizationType.Auto)]
	[DataRow(OptimizationType.Html)]
	[DataRow(OptimizationType.None)]
	public void Selects_Correct_Page_Optimizer(OptimizationType optimizationType)
	{
		const string outFile = "file.html";
		var res =
			new PageResource("/")
			{
				OutFile = outFile,
				OptimizationType = optimizationType
			};
		var expectedType =
			optimizationType switch
			{
				OptimizationType.None => typeof(NullMarkupOptimizer).Name,
				_ => _markupOptimizer.GetType().Name
			};

		var actual = _optimizerSelector.SelectFor(res, outFile);

		Assert.AreEqual(expectedType, actual.GetType().Name, true);
	}


	[DataTestMethod]
	[DataRow(OptimizationType.Auto)]
	[DataRow(OptimizationType.Css)]
	[DataRow(OptimizationType.None)]
	public void Selects_Correct_Css_Optimizer(OptimizationType optimizationType)
	{
		const string outFile = "file.css";
		var res =
			new CssResource("/")
			{
				OutFile = outFile,
				OptimizationType = optimizationType
			};
		var expectedType =
			optimizationType switch
			{
				OptimizationType.None => typeof(NullCssOptimizer).Name,
				_ => _cssOptimizer.GetType().Name
			};

		var actual = _optimizerSelector.SelectFor(res, outFile);

		Assert.AreEqual(expectedType, actual.GetType().Name, true);
	}


	[DataTestMethod]
	[DataRow(OptimizationType.Auto)]
	[DataRow(OptimizationType.Js)]
	[DataRow(OptimizationType.None)]
	public void Selects_Correct_JsRes_Optimizer(OptimizationType optimizationType)
	{
		const string outFile = "file.js";
		var res =
			new JsResource("/")
			{
				OutFile = outFile,
				OptimizationType = optimizationType
			};
		var expectedType =
			optimizationType switch
			{
				OptimizationType.None => typeof(NullJsOptimizer).Name,
				_ => _jsOptimizer.GetType().Name
			};

		var actual = _optimizerSelector.SelectFor(res, outFile);

		Assert.AreEqual(expectedType, actual.GetType().Name, true);
	}


	[DataTestMethod]
	[DataRow(OptimizationType.Auto)]
	[DataRow(OptimizationType.Bin)]
	[DataRow(OptimizationType.None)]
	public void Selects_Correct_BinRes_Optimizer(OptimizationType optimizationType)
	{
		const string outFile = "file.js";
		var res =
			new BinResource("/")
			{
				OutFile = outFile,
				OptimizationType = optimizationType
			};
		var expectedType =
			optimizationType switch
			{
				OptimizationType.None => typeof(NullBinOptimizer).Name,
				_ => _binOptimizer.GetType().Name
			};

		var actual = _optimizerSelector.SelectFor(res, outFile);

		Assert.AreEqual(expectedType, actual.GetType().Name, true);
	}
}
