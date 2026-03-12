namespace Tests.AspNetStatic;

[TestClass]
public class SimpleCliHelperTests
{
    [TestMethod]
    [DataRow(new[] { "no-ssg" }, false)]
    [DataRow(new[] { "other" }, false)]
    [DataRow(new[] { "ssg" }, true)]
    public void Test_HasSsgArg(string[] args, bool expected)
    {
        var actual = args.HasSsgArg();
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [DataRow(new[] { "no-ssg" }, true)]
    [DataRow(new[] { "other" }, false)]
    [DataRow(new[] { "ssg" }, false)]
    public void Test_HasOmitSsgArg(string[] args, bool expected)
    {
        var actual = args.HasOmitSsgArg();
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [DataRow(new[] { "no-ssg" }, false)]
    [DataRow(new[] { "other" }, false)]
    [DataRow(new[] { "ssg" }, false)]
    [DataRow(new[] { "ssg-only" }, true)]
    [DataRow(new[] { "static-only" }, true)]
    [DataRow(new[] { "exit-when-done" }, true)]
    public void Test_HasExitWhenDoneArg(string[] args, bool expected)
    {
        var actual = args.HasExitWhenDoneArg();
        Assert.AreEqual(expected, actual);
    }
}
