namespace Tests.AspNetStatic;

[TestClass]
public class HelpersTests
{
    private static readonly string _indexFileName = "index.html";
    private static readonly string _fileExtension = ".html";
    private static readonly string[] _exclusions = ["index", "default"];



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

        Assert.ThrowsException<ArgumentNullException>(() => nullString!.ToDefaultFileFallback(_exclusions, _indexFileName, _fileExtension));
        Assert.ThrowsException<ArgumentException>(() => emptyString.ToDefaultFileFallback(_exclusions, _indexFileName, _fileExtension));
        Assert.ThrowsException<ArgumentException>(() => whitespace.ToDefaultFileFallback(_exclusions, _indexFileName, _fileExtension));

        Assert.ThrowsException<ArgumentNullException>(() => route.ToDefaultFileFallback(_exclusions, nullString!, _fileExtension));
        Assert.ThrowsException<ArgumentException>(() => route.ToDefaultFileFallback(_exclusions, emptyString, _fileExtension));
        Assert.ThrowsException<ArgumentException>(() => route.ToDefaultFileFallback(_exclusions, whitespace, _fileExtension));

        Assert.ThrowsException<ArgumentNullException>(() => route.ToDefaultFileFallback(_exclusions, _indexFileName, nullString!));
        Assert.ThrowsException<ArgumentException>(() => route.ToDefaultFileFallback(_exclusions, _indexFileName, emptyString));
        Assert.ThrowsException<ArgumentException>(() => route.ToDefaultFileFallback(_exclusions, _indexFileName, whitespace));
    }
}
