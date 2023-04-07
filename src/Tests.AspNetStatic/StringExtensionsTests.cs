namespace Tests.AspNetStatic
{
    [TestClass]
    public class StringExtensionsTests
    {
        #region Starts...

        [DataTestMethod]
        [DataRow("doc", '/', "/doc")]
        [DataRow("/doc", '/', "/doc")]
        [DataRow("", '/', "/")]
        [DataRow("z", 'a', "az")]
        public void Test_AssureStartsWith_Char(string src, char pfx, string expected)
        {
            var actual = src.EnsureStartsWith(pfx);
            Assert.AreEqual(expected, actual, ignoreCase: true);
        }

        [DataTestMethod]
        [DataRow("doc", "/", "/doc")]
        [DataRow("/doc", "/", "/doc")]
        [DataRow("", "/", "/")]
        [DataRow("xyz", "abc", "abcxyz")]
        public void Test_AssureStartsWith_Str(string src, string pfx, string expected)
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
        public void Test_AssureNotStartsWith_Char(string src, char pfx, string expected)
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
        public void Test_AssureNotStartsWith_Str(string src, string pfx, string expected)
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
        public void Test_AssureEndsWith_Char(string src, char sfx, string expected)
        {
            var actual = src.EnsureEndsWith(sfx);
            Assert.AreEqual(expected, actual, ignoreCase: true);
        }

        [DataTestMethod]
        [DataRow("doc", "/", "doc/")]
        [DataRow("/doc/", "/", "/doc/")]
        [DataRow("", "/", "/")]
        [DataRow("abc", "xyz", "abcxyz")]
        public void Test_AssureEndsWith_Str(string src, string sfx, string expected)
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
        public void Test_AssureNotEndsWith_Char(string src, char sfx, string expected)
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
        public void Test_AssureNotEndsWith_Str(string src, string sfx, string expected)
        {
            var actual = src.EnsureNotEndsWith(sfx);
            Assert.AreEqual(expected, actual, ignoreCase: true);
        }

        #endregion
    }
}
