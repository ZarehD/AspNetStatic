namespace Tests.AspNetStatic
{
	[TestClass]
	public class StringExtensionsTests
	{
		#region AreSameText...

		[DataTestMethod]
		[DataRow("abc", "abc", true)]
		[DataRow("a-a", "a-a", true)]
		[DataRow("abc", "xyz", false)]
		[DataRow("a-a", "a-b", false)]
		[DataRow("abc", null, false)]
		[DataRow(null, "xyz", false)]
		[DataRow(null, null, true)]
		public void Test_AreSameText(string? src, string? other, bool expected)
		{
			var actual = src.AreSameText(other);
			Assert.AreEqual(expected, actual);
		}

		#endregion

		#region Starts...

		[DataTestMethod]
		[DataRow("doc", '/', "/doc")]
		[DataRow("/doc", '/', "/doc")]
		[DataRow("", '/', "/")]
		[DataRow("z", 'a', "az")]
		public void Test_EnsureStartsWith_Char(string src, char pfx, string expected)
		{
			var actual = src.EnsureStartsWith(pfx);
			Assert.AreEqual(expected, actual, ignoreCase: true);
		}

		[DataTestMethod]
		[DataRow("doc", "/", "/doc")]
		[DataRow("/doc", "/", "/doc")]
		[DataRow("", "/", "/")]
		[DataRow("xyz", "abc", "abcxyz")]
		public void Test_EnsureStartsWith_Str(string src, string pfx, string expected)
		{
			var actual = src.EnsureStartsWith(pfx);
			Assert.AreEqual(expected, actual, ignoreCase: true);
		}


		[DataTestMethod]
		[DataRow("doc", '/', "doc")]
		[DataRow("/doc", '/', "doc")]
		[DataRow("", '/', "")]
		[DataRow("/", '/', "")]
		[DataRow("az", 'a', "z")]
		public void Test_EnsureNotStartsWith_Char(string src, char pfx, string expected)
		{
			var actual = src.EnsureNotStartsWith(pfx);
			Assert.AreEqual(expected, actual, ignoreCase: true);
		}

		[DataTestMethod]
		[DataRow("doc", "/", "doc")]
		[DataRow("/doc", "/", "doc")]
		[DataRow("", "/", "")]
		[DataRow("/", "/", "")]
		[DataRow("abcxyz", "abc", "xyz")]
		[DataRow("xyz", "abc", "xyz")]
		public void Test_EnsureNotStartsWith_Str(string src, string pfx, string expected)
		{
			var actual = src.EnsureNotStartsWith(pfx);
			Assert.AreEqual(expected, actual, ignoreCase: true);
		}

		#endregion

		#region Ends...

		[DataTestMethod]
		[DataRow("doc", '/', "doc/")]
		[DataRow("/doc/", '/', "/doc/")]
		[DataRow("", '/', "/")]
		[DataRow("a", 'z', "az")]
		public void Test_EnsureEndsWith_Char(string src, char sfx, string expected)
		{
			var actual = src.EnsureEndsWith(sfx);
			Assert.AreEqual(expected, actual, ignoreCase: true);
		}

		[DataTestMethod]
		[DataRow("doc", "/", "doc/")]
		[DataRow("/doc/", "/", "/doc/")]
		[DataRow("", "/", "/")]
		[DataRow("abc", "xyz", "abcxyz")]
		public void Test_EnsureEndsWith_Str(string src, string sfx, string expected)
		{
			var actual = src.EnsureEndsWith(sfx);
			Assert.AreEqual(expected, actual, ignoreCase: true);
		}


		[DataTestMethod]
		[DataRow("doc", '/', "doc")]
		[DataRow("doc/", '/', "doc")]
		[DataRow("", '/', "")]
		[DataRow("/", '/', "")]
		[DataRow("az", 'z', "a")]
		public void Test_EnsureNotEndsWith_Char(string src, char sfx, string expected)
		{
			var actual = src.EnsureNotEndsWith(sfx);
			Assert.AreEqual(expected, actual, ignoreCase: true);
		}

		[DataTestMethod]
		[DataRow("doc", "/", "doc")]
		[DataRow("doc/", "/", "doc")]
		[DataRow("", "/", "")]
		[DataRow("/", "/", "")]
		[DataRow("abcxyz", "xyz", "abc")]
		[DataRow("xyz", "z", "xy")]
		public void Test_EnsureNotEndsWith_Str(string src, string sfx, string expected)
		{
			var actual = src.EnsureNotEndsWith(sfx);
			Assert.AreEqual(expected, actual, ignoreCase: true);
		}

		#endregion
	}
}
