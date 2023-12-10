namespace Tests.AspNetStatic
{
	[TestClass]
	public class StaticResourcesInfoProviderTests
	{
		private readonly static List<PageResource> Test_Pages = new(new PageResource[] { new("/segment/page") });
		private readonly static List<string> Test_Exclusions = new(new[] { "index", "default", "who-this" });
		private readonly static string Test_DefaultFileName = "default-file-name";
		private readonly static string Test_DefaultFileExtension = ".test-extension";

		private class TestStaticResourcesInfoProvider : StaticResourcesInfoProvider
		{
			public TestStaticResourcesInfoProvider()
				: base(Test_Pages, null,
					  Test_DefaultFileName,
					  Test_DefaultFileExtension,
					  Test_Exclusions)
			{
			}

			public new void SetDefaultFileName(string name) => base.SetDefaultFileName(name);
			public new void SetDefaultFileExtension(string extension) => base.SetDefaultFileExtension(extension);
			public new void SetDefaultFileExclusions(string[] newExclusions) => base.SetDefaultFileExclusions(newExclusions);
		}

		private StaticResourcesInfoProvider GetStandardPagesInfoProvider() =>
			new StaticResourcesInfoProvider(
				Test_Pages, null,
				Test_DefaultFileName,
				Test_DefaultFileExtension,
				Test_Exclusions);
		private TestStaticResourcesInfoProvider GetDerivedPagesInfoProvider() =>
			new TestStaticResourcesInfoProvider();

		//--

		[TestMethod]
		public void StandardType_CreateInstance()
		{
			var sut = GetStandardPagesInfoProvider();

			Assert.AreEqual(Test_DefaultFileName, sut.DefaultFileName, ignoreCase: false);
			Assert.AreEqual(Test_DefaultFileExtension, sut.PageFileExtension, ignoreCase: false);

			Assert.AreEqual(Test_Exclusions.Count, sut.DefaultFileExclusions.Length);
			foreach (var exc in Test_Exclusions)
			{
				Assert.IsTrue(sut.DefaultFileExclusions.Contains(exc));
			}

			Assert.AreEqual(Test_Pages.Count, sut.PageResources.Count());
			foreach (var url in Test_Pages.Select(p => p.Route))
			{
				Assert.IsTrue(sut.PageResources.Any(p => p.Route.Equals(url)));
			}
		}

		[TestMethod]
		public void DerivedType_CreateInstance()
		{
			var sut = GetDerivedPagesInfoProvider();

			Assert.AreEqual(Test_DefaultFileName, sut.DefaultFileName, ignoreCase: false);
			Assert.AreEqual(Test_DefaultFileExtension, sut.PageFileExtension, ignoreCase: false);

			Assert.AreEqual(Test_Exclusions.Count, sut.DefaultFileExclusions.Length);
			foreach (var exc in Test_Exclusions)
			{
				Assert.IsTrue(sut.DefaultFileExclusions.Contains(exc));
			}

			Assert.AreEqual(Test_Pages.Count, sut.PageResources.Count());
			foreach (var url in Test_Pages.Select(p => p.Route))
			{
				Assert.IsTrue(sut.PageResources.Any(p => p.Route.Equals(url)));
			}
		}

		//--

		[TestMethod]
		public void DerivedType_SetDefaultFileName_Good()
		{
			var sut = GetDerivedPagesInfoProvider();
			var expected = "test";

			sut.SetDefaultFileName(expected);

			Assert.AreEqual(expected, sut.DefaultFileName);
		}

		[TestMethod]
		public void DerivedType_SetDefaultFileName_Bad()
		{
			var sut = GetDerivedPagesInfoProvider();

			Assert.ThrowsException<ArgumentNullException>(() => sut.SetDefaultFileName(null!));
			Assert.ThrowsException<ArgumentException>(() => sut.SetDefaultFileName(string.Empty));
			Assert.ThrowsException<ArgumentException>(() => sut.SetDefaultFileName(" "));
		}

		//--

		[TestMethod]
		public void DerivedType_SetDefaultFileExtension_Good()
		{
			var sut = GetDerivedPagesInfoProvider();
			var expected = "test";

			sut.SetDefaultFileExtension(expected);

			Assert.AreEqual(expected, sut.PageFileExtension);
		}

		[TestMethod]
		public void DerivedType_SetDefaultFileExtension_Bad()
		{
			var sut = GetDerivedPagesInfoProvider();

			Assert.ThrowsException<ArgumentNullException>(() => sut.SetDefaultFileExtension(null!));
			Assert.ThrowsException<ArgumentException>(() => sut.SetDefaultFileExtension(string.Empty));
			Assert.ThrowsException<ArgumentException>(() => sut.SetDefaultFileExtension(" "));
		}

		//--

		[TestMethod]
		public void DerivedType_SetDefaultFileExclusions_Good()
		{
			var sut = GetDerivedPagesInfoProvider();
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
		public void DerivedType_SetDefaultFileExclusions_Bad()
		{
			var sut = GetDerivedPagesInfoProvider();

			Assert.ThrowsException<ArgumentException>(() => sut.SetDefaultFileExclusions(new string[] { null! }));
			Assert.ThrowsException<ArgumentException>(() => sut.SetDefaultFileExclusions(new[] { string.Empty }));
			Assert.ThrowsException<ArgumentException>(() => sut.SetDefaultFileExclusions(new[] { " " }));
		}
	}
}