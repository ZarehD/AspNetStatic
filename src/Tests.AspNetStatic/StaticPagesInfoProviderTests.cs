namespace Tests.AspNetStatic
{
    [TestClass]
    public class StaticPagesInfoProviderTests
    {
        private readonly static List<PageInfo> Test_Pages = new(new PageInfo[] { new("/segment/page") });
        private readonly static List<string> Test_Exclusions = new(new[] { "index", "default", "who-this" });
        private readonly static bool Test_CreateDefaultFile = true;
        private readonly static string Test_DefaultFileName = "default-file-name";
        private readonly static string Test_DefaultFileExtension = ".test-extension";


        public class TestStaticPagesInfoProvider : StaticPagesInfoProviderBase
        {
            public TestStaticPagesInfoProvider()
            {
                pages.AddRange(Test_Pages);

                exclusions.Clear();
                exclusions.AddRange(Test_Exclusions);

                defaultFileName = Test_DefaultFileName;
                defaultFileExtension = Test_DefaultFileExtension;
            }

            public new void SetDefaultFileName(string name) => base.SetDefaultFileName(name);
            public new void SetDefaultFileExtension(string extension) => base.SetDefaultFileExtension(extension);
            public new void SetDefaultFileExclusions(string[] newExclusions) => base.SetDefaultFileExclusions(newExclusions);
        }


        [TestMethod]
        public void Test_DerivedType()
        {
            var sut = new TestStaticPagesInfoProvider();

            Assert.IsNotNull(sut);

            Assert.AreEqual(Test_DefaultFileName, sut.DefaultFileName, ignoreCase: true);
            Assert.AreEqual(Test_DefaultFileExtension, sut.PageFileExtension, ignoreCase: true);

            Assert.AreEqual(Test_Exclusions.Count, sut.DefaultFileExclusions.Length);
            foreach (var exc in Test_Exclusions)
            {
                Assert.IsTrue(sut.DefaultFileExclusions.Contains(exc));
            }

            Assert.AreEqual(Test_Pages.Count, sut.Pages.Count());
            foreach (var url in Test_Pages.Select(p => p.RelativePath))
            {
                Assert.IsTrue(sut.Pages.Any(
                    p => p.RelativePath.Equals(
                        url, StringComparison.InvariantCultureIgnoreCase)));
            }
        }


        [TestMethod]
        public void Test_SetDefaultFileName_Good()
        {
            var sut = new TestStaticPagesInfoProvider();
            var expected = "test";

            sut.SetDefaultFileName(expected);
            Assert.AreEqual(expected, sut.DefaultFileName, ignoreCase: true);
        }

        [TestMethod]
        public void Test_SetDefaultFileName_Bad()
        {
            var sut = new TestStaticPagesInfoProvider();

            Assert.ThrowsException<ArgumentException>(() => sut.SetDefaultFileName(null!));
            Assert.ThrowsException<ArgumentException>(() => sut.SetDefaultFileName(string.Empty));
            Assert.ThrowsException<ArgumentException>(() => sut.SetDefaultFileName(" "));
        }


        [TestMethod]
        public void Test_SetDefaultFileExtension_Good()
        {
            var sut = new TestStaticPagesInfoProvider();
            var expected = "test";

            sut.SetDefaultFileExtension(expected);
            Assert.AreEqual(expected, sut.PageFileExtension, ignoreCase: true);
        }

        [TestMethod]
        public void Test_SetDefaultFileExtension_Bad()
        {
            var sut = new TestStaticPagesInfoProvider();

            Assert.ThrowsException<ArgumentException>(() => sut.SetDefaultFileExtension(null!));
            Assert.ThrowsException<ArgumentException>(() => sut.SetDefaultFileExtension(string.Empty));
            Assert.ThrowsException<ArgumentException>(() => sut.SetDefaultFileExtension(" "));
        }


        [TestMethod]
        public void Test_SetDefaultFileExclusions_Good()
        {
            var sut = new TestStaticPagesInfoProvider();
            var expected = new[] { "test1", "test2" };

            sut.SetDefaultFileExclusions(expected);
            Assert.AreEqual(expected.Length, sut.DefaultFileExclusions.Length);
            foreach (var x in expected)
            {
                Assert.IsTrue(
                    sut.DefaultFileExclusions.Any(
                        s => s.Equals(x, StringComparison.InvariantCultureIgnoreCase)));
            }
        }

        [TestMethod]
        public void Test_SetDefaultFileExclusions_Bad()
        {
            var sut = new TestStaticPagesInfoProvider();

            Assert.ThrowsException<ArgumentException>(() => sut.SetDefaultFileExclusions(new string[] { null! }));
            Assert.ThrowsException<ArgumentException>(() => sut.SetDefaultFileExclusions(new[] { string.Empty }));
            Assert.ThrowsException<ArgumentException>(() => sut.SetDefaultFileExclusions(new[] { " " }));
        }
    }
}