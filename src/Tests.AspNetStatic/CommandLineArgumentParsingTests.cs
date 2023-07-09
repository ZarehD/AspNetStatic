using AspNetStatic.Models;
using CommandLine;

namespace Tests.AspNetStatic;

[TestClass]
public class CommandLineArgumentParsingTests
{
    private static readonly string[] RequiredArguments = new[] { "--destination-root", "C:\\dev\\solutionDir\\WebApp\\wwwroot" };
    
    [TestMethod]
    public void Test_BooleanArgumentTypeIsGood()
    {
        new Parser().ParseArguments<GenerateStaticPagesOptions>(RequiredArguments.Concat(new[]
                { "--static-only", "false" })) // this does not work
            .WithParsed(o =>
            {
                Assert.IsTrue(o.ExitWhenDone);
            })
            .WithNotParsed(e => { Assert.Fail(); });

        new Parser().ParseArguments<GenerateStaticPagesOptions>(RequiredArguments.Concat(new[]
                { "--static-only" }))
            .WithParsed(o =>
            {
                Assert.IsTrue(o.ExitWhenDone);
            })
            .WithNotParsed(e => { Assert.Fail(); });

        
        new Parser().ParseArguments<GenerateStaticPagesOptions>(RequiredArguments)
            .WithParsed(o =>
            {
                Assert.IsFalse(o.ExitWhenDone);
            })
            .WithNotParsed(e => { Assert.Fail(); });
    }

    [TestMethod]
    public void Test_IntArgumentParsingIsGood()
    {
        
        new Parser().ParseArguments<GenerateStaticPagesOptions>(RequiredArguments.Concat(new[]
                { "--regeneration-interval", "30000" }))
            .WithParsed(o =>
            {
                Assert.AreEqual(o.RegenerationInterval, 30000);
            })
            .WithNotParsed(_ => { Assert.Fail(); });

        new Parser().ParseArguments<GenerateStaticPagesOptions>(RequiredArguments.Concat(new[]
                { "--regeneration-interval", "expectedIntGotString" }))
            .WithParsed(o =>
            {
                Assert.Fail();
            });

        new Parser().ParseArguments<GenerateStaticPagesOptions>(RequiredArguments.Concat(new[]
                { "--regeneration-interval", "null" }))
            .WithParsed(o =>
            {
                Assert.IsNull(o.RegenerationInterval);
            });
    }
    
    [TestMethod]
    public void Test_StringArgumentTypeIsGood()
    {
        new Parser().ParseArguments<GenerateStaticPagesOptions>(new[] 
                { "--destination-root", "C:\\dev\\solutionDir\\WebApp\\wwwroot" })
            .WithParsed(o =>
            {
                Assert.AreEqual(o.DestinationRoot, "C:\\dev\\solutionDir\\WebApp\\wwwroot");
            })
            .WithNotParsed(_ => { Assert.Fail(); });
    }

    [TestMethod]
    public void Test_ParsingWillFail_WhenNonExistentArgumentsAreGiven()
    {
        new Parser().ParseArguments<GenerateStaticPagesOptions>(new[]
                { "--randomGibberish", "foo" })
            .WithParsed(o =>
            {
                Assert.Fail();
            })
            .WithNotParsed(errors =>
            {
                errors.Any(x => x.Tag == ErrorType.UnknownOptionError);
            });
    }
    
    [TestMethod]
    public void Test_ParsingWillIgnoreArgumentsNotStartingWithDash()
    {
        new Parser().ParseArguments<GenerateStaticPagesOptions>(RequiredArguments.Concat(new[]
                { "static-only" }))
            .WithParsed(o =>
            {
                Assert.IsFalse(o.ExitWhenDone);
            })
            .WithNotParsed(errors =>
            {
                Assert.Fail();
            });
    }

    [TestMethod]
    public void Test_ParsingWillFail_WhenSameArgumentIsGivenMultipleTimes()
    {
        new Parser().ParseArguments<GenerateStaticPagesOptions>(new[]
                { "--destination-root", "initial", "--destination-root", "after" })
            .WithNotParsed(errors =>
            {
                Assert.IsTrue(errors.Any(x => x.Tag == ErrorType.RepeatedOptionError));
            });;
    }
}