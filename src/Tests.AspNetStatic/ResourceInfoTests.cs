﻿namespace Tests.AspNetStatic
{
	[TestClass]
	public class ResourceInfoTests
	{
		[TestMethod]
		public void PageInfo_Ctor_1()
		{
			var route = "/route/";
			var queryString = "?parm=value";
			var outFilePathname = "\\route\\index.html";
			var changeFrequency = ChangeFrequency.Daily;
			var indexPriority = 1.0;
			var lastModified = DateTime.UtcNow;
			var optimizerType = OptimizerType.Auto;

			var page =
				new PageResource(route)
				{
					Query = queryString,
					OutFile = outFilePathname,
					ChangeFrequency = changeFrequency,
					IndexPriority = indexPriority,
					LastModified = lastModified,
				};

			Assert.IsNotNull(page);
			Assert.AreEqual(page.Route, route);
			Assert.AreEqual(page.Query, queryString);
			Assert.AreEqual(page.OutFile, outFilePathname);
			Assert.AreEqual(page.ChangeFrequency, changeFrequency);
			Assert.AreEqual(page.IndexPriority, indexPriority);
			Assert.AreEqual(page.LastModified, lastModified);
			Assert.AreEqual(page.SkipOptimization, page.OptimizerType == OptimizerType.None);
			Assert.AreEqual(page.OptimizerType, optimizerType);
			Assert.AreEqual(page.OutputEncoding, EncodingType.UTF8);
		}

		[TestMethod]
		public void PageInfo_Ctor_2()
		{
			var route = "/";
			var queryString = "/x/y";
			var outFilePathname = "\\index.html";
			var changeFrequency = ChangeFrequency.Never;
			var indexPriority = 0;
			var lastModified = DateTime.MinValue;
			var optimizerType = OptimizerType.Html;
			var encoding = EncodingType.ASCII;

			var page =
				new PageResource(route)
				{
					Query = queryString,
					OutFile = outFilePathname,
					OptimizerType = optimizerType,
					OutputEncoding = encoding,
				};

			Assert.IsNotNull(page);
			Assert.AreEqual(page.Route, route);
			Assert.AreEqual(page.Query, queryString);
			Assert.AreEqual(page.OutFile, outFilePathname);
			Assert.AreEqual(page.ChangeFrequency, changeFrequency);
			Assert.AreEqual(page.IndexPriority, indexPriority);
			Assert.AreEqual(page.LastModified, lastModified);
			Assert.AreEqual(page.SkipOptimization, page.OptimizerType == OptimizerType.None);
			Assert.AreEqual(page.OptimizerType, optimizerType);
			Assert.AreEqual(page.OutputEncoding, encoding);
		}

		[TestMethod]
		public void PageInfo_Ctor_3()
		{
			var route = "/";
			var queryString = "/x/y";
			var outFilePathname = "\\index.html";
			var changeFrequency = ChangeFrequency.Never;
			var indexPriority = 0;
			var lastModified = DateTime.MinValue;
			var optimizerType = OptimizerType.None;
			var encoding = EncodingType.UTF32;

			var page =
				new PageResource(route)
				{
					Query = queryString,
					OutFile = outFilePathname,
					OptimizerType = optimizerType,
					OutputEncoding = encoding,
				};

			Assert.IsNotNull(page);
			Assert.AreEqual(page.Route, route);
			Assert.AreEqual(page.Query, queryString);
			Assert.AreEqual(page.OutFile, outFilePathname);
			Assert.AreEqual(page.ChangeFrequency, changeFrequency);
			Assert.AreEqual(page.IndexPriority, indexPriority);
			Assert.AreEqual(page.LastModified, lastModified);
			Assert.AreEqual(page.SkipOptimization, page.OptimizerType == OptimizerType.None);
			Assert.AreEqual(page.OptimizerType, optimizerType);
			Assert.AreEqual(page.OutputEncoding, encoding);
		}


		[TestMethod]
		public void PageInfo_Serialization_TextJson()
		{
			var expected =
				new PageResource("/route/")
				{
					Query = "?parm=value",
					OutFile = "\\route\\index.html",
					ChangeFrequency = ChangeFrequency.Daily,
					IndexPriority = 1.0,
					LastModified = DateTime.UtcNow,
					OptimizerType = OptimizerType.Xml,
					OutputEncoding = EncodingType.BigEndianUnicode,
				};

			var json = System.Text.Json.JsonSerializer.Serialize<PageResource>(expected);
			var actual = System.Text.Json.JsonSerializer.Deserialize<PageResource>(json);

			Assert.AreEqual(expected.Route, actual?.Route);
			Assert.AreEqual(expected.Query, actual?.Query);
			Assert.AreEqual(expected.OutFile, actual?.OutFile);
			Assert.AreEqual(expected.ChangeFrequency, actual?.ChangeFrequency);
			Assert.AreEqual(expected.IndexPriority, actual?.IndexPriority);
			Assert.AreEqual(expected.LastModified, actual?.LastModified);
			Assert.AreEqual(expected.SkipOptimization, actual?.SkipOptimization);
			Assert.AreEqual(expected.OptimizerType, actual?.OptimizerType);
			Assert.AreEqual(expected.OutputEncoding, actual?.OutputEncoding);
		}

		//--

		[TestMethod]
		public void NonPageResourceInfo_Ctor_1()
		{
			var route = "/route/file.xyz";
			var queryString = "?parm=value";
			var outFilePathname = "\\route\\file.xyz";
			var optimizerType = OptimizerType.Auto;

			var resource =
				new PageResource(route)
				{
					Query = queryString,
					OutFile = outFilePathname,
				};

			Assert.IsNotNull(resource);
			Assert.AreEqual(resource.Route, route);
			Assert.AreEqual(resource.Query, queryString);
			Assert.AreEqual(resource.OutFile, outFilePathname);
			Assert.AreEqual(resource.SkipOptimization, resource.OptimizerType == OptimizerType.None);
			Assert.AreEqual(resource.OptimizerType, optimizerType);
			Assert.AreEqual(resource.OutputEncoding, EncodingType.UTF8);
		}

		[TestMethod]
		public void NonPageResourceInfo_Ctor_2()
		{
			var route = "/route/file.xyz";
			var queryString = "x=y";
			var outFilePathname = "\\route\\file.xyz";
			var optimizerType = OptimizerType.Css;
			var encoding = EncodingType.ASCII;

			var resource =
				new PageResource(route)
				{
					Query = queryString,
					OutFile = outFilePathname,
					OptimizerType = optimizerType,
					OutputEncoding = encoding,
				};

			Assert.IsNotNull(resource);
			Assert.AreEqual(resource.Route, route);
			Assert.AreEqual(resource.Query, queryString);
			Assert.AreEqual(resource.OutFile, outFilePathname);
			Assert.AreEqual(resource.SkipOptimization, resource.OptimizerType == OptimizerType.None);
			Assert.AreEqual(resource.OptimizerType, optimizerType);
			Assert.AreEqual(resource.OutputEncoding, encoding);
		}

		[TestMethod]
		public void NonPageResourceInfo_Ctor_3()
		{
			var route = "/route/file.xyz";
			var queryString = "x=y";
			var outFilePathname = "\\route\\file.xyz";
			var optimizerType = OptimizerType.None;
			var encoding = EncodingType.UTF32;

			var resource =
				new PageResource(route)
				{
					Query = queryString,
					OutFile = outFilePathname,
					OptimizerType = optimizerType,
					OutputEncoding = encoding,
				};

			Assert.IsNotNull(resource);
			Assert.AreEqual(resource.Route, route);
			Assert.AreEqual(resource.Query, queryString);
			Assert.AreEqual(resource.OutFile, outFilePathname);
			Assert.AreEqual(resource.SkipOptimization, resource.OptimizerType == OptimizerType.None);
			Assert.AreEqual(resource.OptimizerType, optimizerType);
			Assert.AreEqual(resource.OutputEncoding, encoding);
		}


		[TestMethod]
		public void NonPageResourceInfo_Serialization_TextJson()
		{
			var expected =
				new NonPageResource("/route/file.xyz")
				{
					Query = "parm=value",
					OutFile = "\\route\\file.xyz",
					OptimizerType = OptimizerType.Js,
					OutputEncoding = EncodingType.BigEndianUnicode,
				};

			var json = System.Text.Json.JsonSerializer.Serialize<NonPageResource>(expected);
			var actual = System.Text.Json.JsonSerializer.Deserialize<NonPageResource>(json);

			Assert.AreEqual(expected.Route, actual?.Route);
			Assert.AreEqual(expected.Query, actual?.Query);
			Assert.AreEqual(expected.OutFile, actual?.OutFile);
			Assert.AreEqual(expected.SkipOptimization, actual?.SkipOptimization);
			Assert.AreEqual(expected.OptimizerType, actual?.OptimizerType);
			Assert.AreEqual(expected.OutputEncoding, actual?.OutputEncoding);
		}

		//--

		[TestMethod]
		public void CssResourceInfo_Ctor_1()
		{
			var route = "/route/file.css";
			var queryString = "?parm=value";
			var outFilePathname = "\\route\\file.css";
			var optimizerType = OptimizerType.Auto;

			var resource =
				new CssResource(route)
				{
					Query = queryString,
					OutFile = outFilePathname,
				};

			Assert.IsNotNull(resource);
			Assert.AreEqual(resource.Route, route);
			Assert.AreEqual(resource.Query, queryString);
			Assert.AreEqual(resource.OutFile, outFilePathname);
			Assert.AreEqual(resource.SkipOptimization, resource.OptimizerType == OptimizerType.None);
			Assert.AreEqual(resource.OptimizerType, optimizerType);
			Assert.AreEqual(resource.OutputEncoding, EncodingType.UTF8);
		}

		[TestMethod]
		public void CssResourceInfo_Ctor_2()
		{
			var route = "/route/file.css";
			var queryString = "x=y";
			var outFilePathname = "\\route\\file.css";
			var optimizerType = OptimizerType.Css;
			var encoding = EncodingType.ASCII;

			var resource =
				new CssResource(route)
				{
					Query = queryString,
					OutFile = outFilePathname,
					OptimizerType = optimizerType,
					OutputEncoding = encoding,
				};

			Assert.IsNotNull(resource);
			Assert.AreEqual(resource.Route, route);
			Assert.AreEqual(resource.Query, queryString);
			Assert.AreEqual(resource.OutFile, outFilePathname);
			Assert.AreEqual(resource.SkipOptimization, resource.OptimizerType == OptimizerType.None);
			Assert.AreEqual(resource.OptimizerType, optimizerType);
			Assert.AreEqual(resource.OutputEncoding, encoding);
		}

		[TestMethod]
		public void CssResourceInfo_Ctor_3()
		{
			var route = "/route/file.css";
			var queryString = "x=y";
			var outFilePathname = "\\route\\file.css";
			var optimizerType = OptimizerType.None;
			var encoding = EncodingType.UTF32;

			var resource =
				new CssResource(route)
				{
					Query = queryString,
					OutFile = outFilePathname,
					OptimizerType = optimizerType,
					OutputEncoding = encoding,
				};

			Assert.IsNotNull(resource);
			Assert.AreEqual(resource.Route, route);
			Assert.AreEqual(resource.Query, queryString);
			Assert.AreEqual(resource.OutFile, outFilePathname);
			Assert.AreEqual(resource.SkipOptimization, resource.OptimizerType == OptimizerType.None);
			Assert.AreEqual(resource.OptimizerType, optimizerType);
			Assert.AreEqual(resource.OutputEncoding, encoding);
		}


		[TestMethod]
		public void CssResourceInfo_Serialization_TextJson()
		{
			var expected =
				new CssResource("/route/file.css")
				{
					Query = "parm=value",
					OutFile = "\\route\\file.css",
					OptimizerType = OptimizerType.Css,
					OutputEncoding = EncodingType.BigEndianUnicode,
				};

			var json = System.Text.Json.JsonSerializer.Serialize<CssResource>(expected);
			var actual = System.Text.Json.JsonSerializer.Deserialize<CssResource>(json);

			Assert.AreEqual(expected.Route, actual?.Route);
			Assert.AreEqual(expected.Query, actual?.Query);
			Assert.AreEqual(expected.OutFile, actual?.OutFile);
			Assert.AreEqual(expected.SkipOptimization, actual?.SkipOptimization);
			Assert.AreEqual(expected.OptimizerType, actual?.OptimizerType);
			Assert.AreEqual(expected.OutputEncoding, actual?.OutputEncoding);
		}

		//--

		[TestMethod]
		public void JsResourceInfo_Ctor_1()
		{
			var route = "/route/file.js";
			var queryString = "?parm=value";
			var outFilePathname = "\\route\\file.js";
			var optimizerType = OptimizerType.Auto;

			var resource =
				new JsResource(route)
				{
					Query = queryString,
					OutFile = outFilePathname,
				};

			Assert.IsNotNull(resource);
			Assert.AreEqual(resource.Route, route);
			Assert.AreEqual(resource.Query, queryString);
			Assert.AreEqual(resource.OutFile, outFilePathname);
			Assert.AreEqual(resource.SkipOptimization, resource.OptimizerType == OptimizerType.None);
			Assert.AreEqual(resource.OptimizerType, optimizerType);
			Assert.AreEqual(resource.OutputEncoding, EncodingType.UTF8);
		}

		[TestMethod]
		public void JsResourceInfo_Ctor_2()
		{
			var route = "/route/file.js";
			var queryString = "x=y";
			var outFilePathname = "\\route\\file.js";
			var optimizerType = OptimizerType.Js;
			var encoding = EncodingType.ASCII;

			var resource =
				new JsResource(route)
				{
					Query = queryString,
					OutFile = outFilePathname,
					OptimizerType = optimizerType,
					OutputEncoding = encoding,
				};

			Assert.IsNotNull(resource);
			Assert.AreEqual(resource.Route, route);
			Assert.AreEqual(resource.Query, queryString);
			Assert.AreEqual(resource.OutFile, outFilePathname);
			Assert.AreEqual(resource.SkipOptimization, resource.OptimizerType == OptimizerType.None);
			Assert.AreEqual(resource.OptimizerType, optimizerType);
			Assert.AreEqual(resource.OutputEncoding, encoding);
		}

		[TestMethod]
		public void JsResourceInfo_Ctor_3()
		{
			var route = "/route/file.js";
			var queryString = "x=y";
			var outFilePathname = "\\route\\file.js";
			var optimizerType = OptimizerType.None;
			var encoding = EncodingType.UTF32;

			var resource =
				new JsResource(route)
				{
					Query = queryString,
					OutFile = outFilePathname,
					OptimizerType = optimizerType,
					OutputEncoding = encoding,
				};

			Assert.IsNotNull(resource);
			Assert.AreEqual(resource.Route, route);
			Assert.AreEqual(resource.Query, queryString);
			Assert.AreEqual(resource.OutFile, outFilePathname);
			Assert.AreEqual(resource.SkipOptimization, resource.OptimizerType == OptimizerType.None);
			Assert.AreEqual(resource.OptimizerType, optimizerType);
			Assert.AreEqual(resource.OutputEncoding, encoding);
		}


		[TestMethod]
		public void JsResourceInfo_Serialization_TextJson()
		{
			var expected =
				new JsResource("/route/file.js")
				{
					Query = "parm=value",
					OutFile = "\\route\\file.js",
					OptimizerType = OptimizerType.Js,
					OutputEncoding = EncodingType.BigEndianUnicode,
				};

			var json = System.Text.Json.JsonSerializer.Serialize<JsResource>(expected);
			var actual = System.Text.Json.JsonSerializer.Deserialize<JsResource>(json);

			Assert.AreEqual(expected.Route, actual?.Route);
			Assert.AreEqual(expected.Query, actual?.Query);
			Assert.AreEqual(expected.OutFile, actual?.OutFile);
			Assert.AreEqual(expected.SkipOptimization, actual?.SkipOptimization);
			Assert.AreEqual(expected.OptimizerType, actual?.OptimizerType);
			Assert.AreEqual(expected.OutputEncoding, actual?.OutputEncoding);
		}

		//--

		[TestMethod]
		public void BinResourceInfo_Ctor_1()
		{
			var route = "/route/file.bin";
			var queryString = "?parm=value";
			var outFilePathname = "\\route\\file.bin";

			var resource =
				new BinResource(route)
				{
					Query = queryString,
					OutFile = outFilePathname,
				};

			Assert.IsNotNull(resource);
			Assert.AreEqual(resource.Route, route);
			Assert.AreEqual(resource.Query, queryString);
			Assert.AreEqual(resource.OutFile, outFilePathname);
			Assert.AreEqual(resource.SkipOptimization, resource.OptimizerType == OptimizerType.None);
			Assert.AreEqual(resource.OptimizerType, OptimizerType.None);
			Assert.AreEqual(resource.OutputEncoding, EncodingType.Default);
		}

		[TestMethod]
		public void BinResourceInfo_Ctor_2()
		{
			var route = "/route/file.bin";
			var queryString = "x=y";
			var outFilePathname = "\\route\\file.bin";

			var resource =
				new BinResource(route)
				{
					Query = queryString,
					OutFile = outFilePathname,
				};

			Assert.IsNotNull(resource);
			Assert.AreEqual(resource.Route, route);
			Assert.AreEqual(resource.Query, queryString);
			Assert.AreEqual(resource.OutFile, outFilePathname);
			Assert.AreEqual(resource.SkipOptimization, resource.OptimizerType == OptimizerType.None);
			Assert.AreEqual(resource.OptimizerType, OptimizerType.None);
			Assert.AreEqual(resource.OutputEncoding, EncodingType.Default);
		}

		[TestMethod]
		public void BinResourceInfo_Ctor_3()
		{
			var route = "/route/file.bin";
			var queryString = "x=y";
			var outFilePathname = "\\route\\file.bin";

			var resource =
				new BinResource(route)
				{
					Query = queryString,
					OutFile = outFilePathname
				};

			Assert.IsNotNull(resource);
			Assert.AreEqual(resource.Route, route);
			Assert.AreEqual(resource.Query, queryString);
			Assert.AreEqual(resource.OutFile, outFilePathname);
			Assert.AreEqual(resource.SkipOptimization, resource.OptimizerType == OptimizerType.None);
			Assert.AreEqual(resource.OptimizerType, OptimizerType.None);
			Assert.AreEqual(resource.OutputEncoding, EncodingType.Default);
		}


		[TestMethod]
		public void BinResourceInfo_Serialization_TextBinon()
		{
			var expected =
				new BinResource("/route/file.bin")
				{
					Query = "parm=value",
					OutFile = "\\route\\file.bin",
				};

			var json = System.Text.Json.JsonSerializer.Serialize<BinResource>(expected);
			var actual = System.Text.Json.JsonSerializer.Deserialize<BinResource>(json);

			Assert.AreEqual(expected.Route, actual?.Route);
			Assert.AreEqual(expected.Query, actual?.Query);
			Assert.AreEqual(expected.OutFile, actual?.OutFile);
			Assert.AreEqual(expected.SkipOptimization, actual?.SkipOptimization);
			Assert.AreEqual(expected.OptimizerType, actual?.OptimizerType);
			Assert.AreEqual(expected.OutputEncoding, actual?.OutputEncoding);
		}
	}
}
