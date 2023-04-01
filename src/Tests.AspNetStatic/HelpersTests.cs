namespace Tests.AspNetStatic
{
    [TestClass]
    public class HelpersTests
    {
        private static readonly string _indexFileName = "index.html";
        private static readonly string _fileExtension = ".html";
        private static readonly string[] _exclusions = new[] { "index", "default" };
        private static readonly List<PageInfo> _pages =
            new(new PageInfo[]
            {
                new("/"),
                new("/privacy"),
                new("/blog/"),
                new("/blog/article1"),
                new("/blog/categories/"),
                new("/blogs/blog/article2"),
                new("/blogs/blog/categories/"),
                new("/blogs/blog/articles/"),
                new("/blogs/blog/articles/article1"),
                new("docs/"),
                new("docs/page1"),
                new("docs/page2/"),
            });



        #region Pages Includes Route...

        [DataTestMethod]
        [DataRow("/", true)]
        [DataRow("abc/", false)]
        [DataRow("/abc", false)]
        [DataRow("/abc/", false)]
        [DataRow("/abc/xyz", false)]
        [DataRow("/blog/", true)]
        [DataRow("/blog/article1", true)]
        [DataRow("/blog/categories/", true)]
        [DataRow("docs/", true)]
        [DataRow("docs/page1", true)]
        [DataRow("docs/page2/", true)]
        public void Test_PagesIncludesRoute(string route, bool expected)
        {
            var actual = _pages.IncludesRoute(route);
            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Fallback To DefaultFile...

        [DataTestMethod]
        [DataRow("/", "/index.html")]
        [DataRow("/index", "/index.html")]
        [DataRow("/page", "/page/index.html")]
        [DataRow("/page/", "/page/index.html")]
        [DataRow("/page/index", "/page/index.html")]
        public void Test_ToDefaultFileFallback(string route, string expected)
        {
            var actual = route.ToDefaultFileFallback(_exclusions, _indexFileName, _fileExtension);
            Assert.AreEqual(expected, actual, ignoreCase: true);
        }


        [TestMethod]
        public void Test_ToDefaultFileFallback_BadInputs()
        {
            var route = "route";
            var nullString = default(string);
            var emptyString = string.Empty;
            var whitespace = " ";

            Assert.ThrowsException<ArgumentException>(() => nullString!.ToDefaultFileFallback(_exclusions, _indexFileName, _fileExtension));
            Assert.ThrowsException<ArgumentException>(() => emptyString.ToDefaultFileFallback(_exclusions, _indexFileName, _fileExtension));
            Assert.ThrowsException<ArgumentException>(() => whitespace.ToDefaultFileFallback(_exclusions, _indexFileName, _fileExtension));

            Assert.ThrowsException<ArgumentException>(() => route.ToDefaultFileFallback(_exclusions, nullString!, _fileExtension));
            Assert.ThrowsException<ArgumentException>(() => route.ToDefaultFileFallback(_exclusions, emptyString, _fileExtension));
            Assert.ThrowsException<ArgumentException>(() => route.ToDefaultFileFallback(_exclusions, whitespace, _fileExtension));

            Assert.ThrowsException<ArgumentException>(() => route.ToDefaultFileFallback(_exclusions, _indexFileName, nullString!));
            Assert.ThrowsException<ArgumentException>(() => route.ToDefaultFileFallback(_exclusions, _indexFileName, emptyString));
            Assert.ThrowsException<ArgumentException>(() => route.ToDefaultFileFallback(_exclusions, _indexFileName, whitespace));
        }

        #endregion
    }
}
