namespace Tests.AspNetStatic
{
	[TestClass]
	public class StringExtensionsTests
	{
		#region HasSameText...

		[DataTestMethod]
		[DataRow("abc", "abc", true)]
		[DataRow("a-a", "a-a", true)]
		[DataRow("abc", "xyz", false)]
		[DataRow("a-a", "a-b", false)]
		[DataRow("abc", null, false)]
		[DataRow(null, "xyz", false)]
		[DataRow(null, null, true)]
		public void Test_HasSameText(string? src, string? other, bool expected)
		{
			var actual = src.HasSameText(other);
			Assert.AreEqual(expected, actual);
		}

		#endregion

		#region StartsWith...

		[DataTestMethod]
		[DataRow("doc", '/', false, "/doc")]
		[DataRow("/doc", '/', false, "/doc")]
		[DataRow("", '/', false, "/")]
		[DataRow("z", 'a', false, "az")]
		[DataRow(null, 'a', true, "")]
		[DataRow("", 'a', true, "")]
		[DataRow(" ", 'a', true, "a ")]
		public void Test_EnsureStartsWith_Char(string? src, char pfx, bool onlyIfHasValue, string expected)
		{
			var actual = src.EnsureStartsWith(pfx, onlyIfHasValue);
			Assert.AreEqual(expected, actual, ignoreCase: true);
		}

		[DataTestMethod]
		[DataRow("doc", "/", false, "/doc")]
		[DataRow("/doc", "/", false, "/doc")]
		[DataRow("", "/", false, "/")]
		[DataRow("xyz", "abc", false, "abcxyz")]
		[DataRow(null, "abc", true, "")]
		[DataRow("", "abc", true, "")]
		[DataRow(" ", "abc", true, "abc ")]
		public void Test_EnsureStartsWith_Str(string? src, string pfx, bool onlyIfHasValue, string expected)
		{
			var actual = src.EnsureStartsWith(pfx, onlyIfHasValue);
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

		#region EndsWith...

		[DataTestMethod]
		[DataRow("doc", '/', false, "doc/")]
		[DataRow("/doc/", '/', false, "/doc/")]
		[DataRow("", '/', false, "/")]
		[DataRow("a", 'z', false, "az")]
		[DataRow(null, 'x', true, "")]
		[DataRow("", 'x', true, "")]
		[DataRow(" ", 'x', true, " x")]
		public void Test_EnsureEndsWith_Char(string? src, char sfx, bool onlyIfHasValue, string expected)
		{
			var actual = src.EnsureEndsWith(sfx, onlyIfHasValue);
			Assert.AreEqual(expected, actual, ignoreCase: true);
		}

		[DataTestMethod]
		[DataRow("doc", "/", false, "doc/")]
		[DataRow("/doc/", "/", false, "/doc/")]
		[DataRow("", "/", false, "/")]
		[DataRow("abc", "xyz", false, "abcxyz")]
		[DataRow(null, "xyz", true, "")]
		[DataRow("", "xyz", true, "")]
		[DataRow(" ", "xyz", true, " xyz")]
		public void Test_EnsureEndsWith_Str(string src, string sfx, bool onlyIfHasValue, string expected)
		{
			var actual = src.EnsureEndsWith(sfx, onlyIfHasValue);
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
