#define ENABLE_STATIC_PAGE_FALLBACK

using AspNetStatic;
using AspNetStatic.Models;
using CommandLine;
using PartialStaticSite;

GenerateStaticPagesOptions? options = null;
new Parser().ParseArguments<GenerateStaticPagesOptions>(args)
	.WithNotParsed(errors =>
	{
		throw new ArgumentException("Cannot parse commandline arguments!");
	})
	.WithParsed(x => options = x);
// var exitWhenDone = args.HasExitAfterStaticGenerationParameter();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(
	options =>
	{
		options.LowercaseUrls = true;
		options.LowercaseQueryStrings = true;
		options.AppendTrailingSlash = false;
	});

builder.Services.AddRazorPages();

builder.Services.AddSingleton<IStaticPagesInfoProvider>(
	new StaticPagesInfoProvider(SampleStaticPages.GetCollection()));

#region app.UseStaticPageFallback()
#if ENABLE_STATIC_PAGE_FALLBACK

builder.Services.AddStaticPageFallback(
	cfg =>
	{
		cfg.AlwaysDefaultFile = false;
	});

#endif
#endregion


var app = builder.Build();

var intervalStr = app.Configuration["AspNetStatic:RegenTimer"];
TimeSpan? regenInterval = TimeSpan.TryParse(intervalStr, out var ts) ? ts : null;


if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
}

app.UseHttpsRedirection();

#region app.UseStaticPageFallback()
#if ENABLE_STATIC_PAGE_FALLBACK

app.UseStaticPageFallback();

#endif
#endregion

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.GenerateStaticPages(
	app.Environment.WebRootPath,
	exitWhenDone: options.ExitWhenDone,
	alwaysDefaultFile: options.AlwaysDefaultFile,
	dontUpdateLinks: options.DontUpdateLinks,
	dontOptimizeContent: options.DontOptimizeContent,
	regenerationInterval: regenInterval);

app.Run();

#if DEBUG
Console.WriteLine("Press any Key to exit...");
Console.ReadKey();
#endif
