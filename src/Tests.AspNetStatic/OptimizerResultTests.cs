using AspNetStatic.Optimizer;
using WebMarkupMin.Core;

namespace Tests.AspNetStatic;

[TestClass]
public class OptimizerResultTests
{
	// OptimizerResult<TContent>

	[TestMethod]
	public void OptimizerResult_Ctor_Sets_Content()
	{
		var expectedContent = "test content";

		var result = new OptimizerResult<string>(expectedContent);

		Assert.AreEqual(expectedContent, result.OptimizedContent);
	}

	[TestMethod]
	public void OptimizerResult_Ctor_Sets_ContentAndErrors()
	{
		var expectedContent = "test content";
		var expectedErrors = new List<OptimizerErrorInfo>() { new("error message") }.ToArray();

		var result = new OptimizerResult<string>(expectedContent, expectedErrors);

		Assert.AreEqual(expectedContent, result.OptimizedContent);
		Assert.AreEqual(expectedErrors.Length, result.Errors.Count);
		Assert.AreEqual(expectedErrors.First(), result.Errors.First());
	}

	[TestMethod]
	public void OptimizerResult_Ctor_Sets_ContentAndErrorsAndWarnings()
	{
		var expectedContent = "test content";
		var expectedErrors = new List<OptimizerErrorInfo>() { new("error message") }.ToArray();
		var expectedWarnings = new List<OptimizerErrorInfo>() { new("warning message") }.ToArray();

		var result = new OptimizerResult<string>(expectedContent, expectedErrors, expectedWarnings);

		Assert.AreEqual(expectedContent, result.OptimizedContent);

		Assert.AreEqual(expectedErrors.Length, result.Errors.Count);
		Assert.AreEqual(expectedErrors.First(), result.Errors.First());

		Assert.AreEqual(expectedWarnings.Length, result.Warnings.Count);
		Assert.AreEqual(expectedWarnings.First(), result.Warnings.First());
	}


	[TestMethod]
	public void OptimizerResult_w_Errors_HasErrors()
	{
		var result = new OptimizerResult<string>([new("error message")]);

		Assert.IsTrue(result.HasErrors);
		Assert.IsNull(result.OptimizedContent);
	}

	[TestMethod]
	public void OptimizerResult_w_No_Errors_Not_HasErrors()
	{
		var result = new OptimizerResult<string>([]);

		Assert.IsFalse(result.HasErrors);
		Assert.IsNull(result.OptimizedContent);
	}


	[TestMethod]
	public void OptimizerResult_w_Warnings_HasWarnings()
	{
		var result =
			new OptimizerResult<string>(
				"", [],
				[new("warning message")]);

		Assert.IsTrue(result.HasWarnings);
	}

	[TestMethod]
	public void OptimizerResult_w_No_Warnings_Not_HasWarnings()
	{
		var result = new OptimizerResult<string>("", [], []);

		Assert.IsFalse(result.HasWarnings);
	}


	// OptimizerErrorInfo

	[TestMethod]
	public void OptimizerErrorInfo_Ctor()
	{
		string expCategory = nameof(expCategory);
		string expMessage = nameof(expMessage);
		var expLineNum = 10;
		var expColNum = 20;
		string expSource = nameof(expSource);

		var actual = new OptimizerErrorInfo(
			expCategory, expMessage,
			expLineNum, expColNum,
			expSource);

		Assert.AreEqual(expCategory, actual.Category);
		Assert.AreEqual(expMessage, actual.Message);
		Assert.AreEqual(expLineNum, actual.LineNumber);
		Assert.AreEqual(expColNum, actual.ColumnNumber);
		Assert.AreEqual(expSource, actual.SourceFragment);
	}

	[TestMethod]
	public void OptimizerErrorInfo_Type_Cast_to_String()
	{
		string expCategory = nameof(expCategory);
		string expMessage = nameof(expMessage);
		var expLineNum = 10;
		var expColNum = 20;
		string expSource = nameof(expSource);

		var obj = new OptimizerErrorInfo(
			expCategory, expMessage,
			expLineNum, expColNum,
			expSource);

		var actual = (string) obj;

		Assert.IsTrue(actual.Contains(expCategory));
		Assert.IsTrue(actual.Contains(expMessage));
		Assert.IsTrue(actual.Contains($"L:{expLineNum}"));
		Assert.IsTrue(actual.Contains($"C:{expColNum}"));
		Assert.IsTrue(actual.Contains($"S:{expSource}"));
	}

	[TestMethod]
	public void OptimizerErrorInfo_ToString()
	{
		string expCategory = nameof(expCategory);
		string expMessage = nameof(expMessage);
		var expLineNum = 10;
		var expColNum = 20;
		string expSource = nameof(expSource);

		var obj = new OptimizerErrorInfo(
			expCategory, expMessage,
			expLineNum, expColNum,
			expSource);

		var actual = obj.ToString();

		Assert.IsTrue(actual.Contains(expCategory));
		Assert.IsTrue(actual.Contains(expMessage));
		Assert.IsTrue(actual.Contains($"L:{expLineNum}"));
		Assert.IsTrue(actual.Contains($"C:{expColNum}"));
		Assert.IsTrue(actual.Contains($"S:{expSource}"));
	}

	[TestMethod]
	public void OptimizerErrorInfo_Type_Cast_from_MinificationErrorInfo()
	{
		string expCategory = nameof(expCategory);
		string expMessage = nameof(expMessage);
		var expLineNum = 10;
		var expColNum = 20;
		string expSource = nameof(expSource);

		var expected = new MinificationErrorInfo(
			expCategory, expMessage,
			expLineNum, expColNum,
			expSource);

		var actual = (OptimizerErrorInfo) expected;

		Assert.IsNotNull(actual);
		Assert.AreEqual(expCategory, actual.Category);
		Assert.AreEqual(expMessage, actual.Message);
		Assert.AreEqual(expLineNum, actual.LineNumber);
		Assert.AreEqual(expColNum, actual.ColumnNumber);
		Assert.AreEqual(expSource, actual.SourceFragment);
	}

	[TestMethod]
	public void OptimizerErrorInfo_Type_Cast_from_MinificationErrorInfo_Collection()
	{
		string expCategory = nameof(expCategory);
		string expMessage = nameof(expMessage);
		var expLineNum = 10;
		var expColNum = 20;
		string expSource = nameof(expSource);

		var expected =
			new List<MinificationErrorInfo>()
			{
				new(expCategory, expMessage,
					expLineNum, expColNum,
					expSource)
			};

		var actual = expected.Select(x => (OptimizerErrorInfo) x).First();

		Assert.IsNotNull(actual);
		Assert.AreEqual(expCategory, actual.Category);
		Assert.AreEqual(expMessage, actual.Message);
		Assert.AreEqual(expLineNum, actual.LineNumber);
		Assert.AreEqual(expColNum, actual.ColumnNumber);
		Assert.AreEqual(expSource, actual.SourceFragment);
	}


	// MarkupOptimizerResult

	[TestMethod]
	public void MarkupOptimizerResult_Type_Cast_from_MarkupMinificationResult()
	{
		string expContent = nameof(expContent);

		string expCategory = nameof(expCategory);
		string expMessage = nameof(expMessage);
		var expLineNum = 10;
		var expColNum = 20;
		string expSource = nameof(expSource);

		var expError =
			new MinificationErrorInfo(
				expCategory, expMessage,
				expLineNum, expColNum,
				expSource);

		var expWarning =
			new MinificationErrorInfo(
				expCategory, expMessage,
				expLineNum, expColNum,
				expSource);

		var expErrors = new List<MinificationErrorInfo>() { expError };
		var expWarnings = new List<MinificationErrorInfo>() { expWarning };
		var expected = new MarkupMinificationResult(expContent, expErrors, expWarnings);


		var actual = (MarkupOptimizerResult) expected;


		Assert.AreEqual(expected.MinifiedContent, actual.OptimizedContent);

		Assert.AreEqual(expected.Errors.Count, actual.Errors.Count);
		Assert.AreEqual(expError.Category, actual.Errors.First().Category);
		Assert.AreEqual(expError.Message, actual.Errors.First().Message);

		Assert.AreEqual(expected.Warnings.Count, actual.Warnings.Count);
		Assert.AreEqual(expWarning.Category, actual.Warnings.First().Category);
		Assert.AreEqual(expWarning.Message, actual.Warnings.First().Message);
	}


	// CssOptimizerResult

	[TestMethod]
	public void CssOptimizerResult_Type_Cast_from_CssMinificationResult()
	{
		string expContent = nameof(expContent);

		string expCategory = nameof(expCategory);
		string expMessage = nameof(expMessage);
		var expLineNum = 10;
		var expColNum = 20;
		string expSource = nameof(expSource);

		var expError =
			new MinificationErrorInfo(
				expCategory, expMessage,
				expLineNum, expColNum,
				expSource);

		var expWarning =
			new MinificationErrorInfo(
				expCategory, expMessage,
				expLineNum, expColNum,
				expSource);

		var expErrors = new List<MinificationErrorInfo>() { expError };
		var expWarnings = new List<MinificationErrorInfo>() { expWarning };
		var expected = new CodeMinificationResult(expContent, expErrors, expWarnings);


		var actual = (CssOptimizerResult) expected;


		Assert.AreEqual(expected.MinifiedContent, actual.OptimizedContent);

		Assert.AreEqual(expected.Errors.Count, actual.Errors.Count);
		Assert.AreEqual(expError.Category, actual.Errors.First().Category);
		Assert.AreEqual(expError.Message, actual.Errors.First().Message);

		Assert.AreEqual(expected.Warnings.Count, actual.Warnings.Count);
		Assert.AreEqual(expWarning.Category, actual.Warnings.First().Category);
		Assert.AreEqual(expWarning.Message, actual.Warnings.First().Message);
	}


	// JsOptimizerResult

	[TestMethod]
	public void JsOptimizerResult_Type_Cast_from_JsMinificationResult()
	{
		string expContent = nameof(expContent);

		string expCategory = nameof(expCategory);
		string expMessage = nameof(expMessage);
		var expLineNum = 10;
		var expColNum = 20;
		string expSource = nameof(expSource);

		var expError =
			new MinificationErrorInfo(
				expCategory, expMessage,
				expLineNum, expColNum,
				expSource);

		var expWarning =
			new MinificationErrorInfo(
				expCategory, expMessage,
				expLineNum, expColNum,
				expSource);

		var expErrors = new List<MinificationErrorInfo>() { expError };
		var expWarnings = new List<MinificationErrorInfo>() { expWarning };
		var expected = new CodeMinificationResult(expContent, expErrors, expWarnings);


		var actual = (JsOptimizerResult) expected;


		Assert.AreEqual(expected.MinifiedContent, actual.OptimizedContent);

		Assert.AreEqual(expected.Errors.Count, actual.Errors.Count);
		Assert.AreEqual(expError.Category, actual.Errors.First().Category);
		Assert.AreEqual(expError.Message, actual.Errors.First().Message);

		Assert.AreEqual(expected.Warnings.Count, actual.Warnings.Count);
		Assert.AreEqual(expWarning.Category, actual.Warnings.First().Category);
		Assert.AreEqual(expWarning.Message, actual.Warnings.First().Message);
	}
}
