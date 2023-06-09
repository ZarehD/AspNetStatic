﻿using System.Text;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Tests.AspNetStatic
{
	[TestClass]
	public class PageInfoTests
	{
		[TestMethod]
		public void Test_Ctor_1()
		{
			var route = "/route/";
			var queryString = "?parm=value";
			var outFilePathname = "\\route\\index.html";
			var changeFrequency = ChangeFrequency.Daily;
			var indexPriority = 1.0;
			var lastModified = DateTime.UtcNow;
			var optimizerType = OptimizerType.Auto;

			var page =
				new PageInfo(route)
				{
					Query = queryString,
					OutFile = outFilePathname,
					ChangeFrequency = changeFrequency,
					IndexPriority = indexPriority,
					LastModified = lastModified,
				};

			Assert.IsNotNull(page);
			Assert.AreEqual(page.Route, route, ignoreCase: true);
			Assert.AreEqual(page.Query, queryString, ignoreCase: true);
			Assert.AreEqual(page.OutFile, outFilePathname, ignoreCase: true);
			Assert.AreEqual(page.ChangeFrequency, changeFrequency);
			Assert.AreEqual(page.IndexPriority, indexPriority);
			Assert.AreEqual(page.LastModified, lastModified);
			Assert.AreEqual(page.SkipOptimization, page.OptimizerType == OptimizerType.None);
			Assert.AreEqual(page.OptimizerType, optimizerType);
			Assert.AreEqual(page.OutputEncoding, EncodingType.UTF8);
		}

		[TestMethod]
		public void Test_Ctor_2()
		{
			var route = "/";
			var queryString = "/x/y";
			var pageFilePathname = "\\index.html";
			var changeFrequency = ChangeFrequency.Never;
			var indexPriority = 0;
			var lastModified = DateTime.MinValue;
			var optimizerType = OptimizerType.Html;
			var encoding = EncodingType.ASCII;

			var page =
				new PageInfo(route)
				{
					Query = queryString,
					OutFile = pageFilePathname,
					OptimizerType = optimizerType,
					OutputEncoding = encoding,
				};

			Assert.IsNotNull(page);
			Assert.AreEqual(page.Route, route, ignoreCase: true);
			Assert.AreEqual(page.Query, queryString, ignoreCase: true);
			Assert.AreEqual(page.OutFile, pageFilePathname, ignoreCase: true);
			Assert.AreEqual(page.ChangeFrequency, changeFrequency);
			Assert.AreEqual(page.IndexPriority, indexPriority);
			Assert.AreEqual(page.LastModified, lastModified);
			Assert.AreEqual(page.SkipOptimization, page.OptimizerType == OptimizerType.None);
			Assert.AreEqual(page.OptimizerType, optimizerType);
			Assert.AreEqual(page.OutputEncoding, encoding);
		}

		[TestMethod]
		public void Test_Ctor_3()
		{
			var route = "/";
			var queryString = "/x/y";
			var pageFilePathname = "\\index.html";
			var changeFrequency = ChangeFrequency.Never;
			var indexPriority = 0;
			var lastModified = DateTime.MinValue;
			var optimizerType = OptimizerType.None;
			var encoding = EncodingType.UTF32;

			var page =
				new PageInfo(route)
				{
					Query = queryString,
					OutFile = pageFilePathname,
					OptimizerType = optimizerType,
					OutputEncoding = encoding,
				};

			Assert.IsNotNull(page);
			Assert.AreEqual(page.Route, route, ignoreCase: true);
			Assert.AreEqual(page.Query, queryString, ignoreCase: true);
			Assert.AreEqual(page.OutFile, pageFilePathname, ignoreCase: true);
			Assert.AreEqual(page.ChangeFrequency, changeFrequency);
			Assert.AreEqual(page.IndexPriority, indexPriority);
			Assert.AreEqual(page.LastModified, lastModified);
			Assert.AreEqual(page.SkipOptimization, page.OptimizerType == OptimizerType.None);
			Assert.AreEqual(page.OptimizerType, optimizerType);
			Assert.AreEqual(page.OutputEncoding, encoding);
		}


		[TestMethod]
		public void Test_Serialization_TextJson()
		{
			var expected =
				new PageInfo("/route/")
				{
					Query = "?parm=value",
					OutFile = "\\route\\index.html",
					ChangeFrequency = ChangeFrequency.Daily,
					IndexPriority = 1.0,
					LastModified = DateTime.UtcNow,
					OptimizerType = OptimizerType.Xml,
					OutputEncoding = EncodingType.BigEndianUnicode,
				};

			var json = System.Text.Json.JsonSerializer.Serialize<PageInfo>(expected);
			var actual = System.Text.Json.JsonSerializer.Deserialize<PageInfo>(json);

			Assert.AreEqual(expected.Route, actual?.Route, ignoreCase: true);
			Assert.AreEqual(expected.Query, actual?.Query, ignoreCase: true);
			Assert.AreEqual(expected.OutFile, actual?.OutFile, ignoreCase: true);
			Assert.AreEqual(expected.ChangeFrequency, actual?.ChangeFrequency);
			Assert.AreEqual(expected.IndexPriority, actual?.IndexPriority);
			Assert.AreEqual(expected.LastModified, actual?.LastModified);
			Assert.AreEqual(expected.SkipOptimization, actual?.SkipOptimization);
			Assert.AreEqual(expected.OptimizerType, actual?.OptimizerType);
			Assert.AreEqual(expected.OutputEncoding, actual?.OutputEncoding);
		}
	}
}
