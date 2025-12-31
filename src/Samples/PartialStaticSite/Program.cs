#define ENABLE_STATIC_PAGE_FALLBACK

using AspNetStatic;
using AspNetStaticContrib.AspNetStatic;
using Microsoft.Extensions.Hosting;
using PartialStaticSite;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(
	options =>
	{
		options.LowercaseUrls = true;
		options.LowercaseQueryStrings = true;
		options.AppendTrailingSlash = false;
	});

builder.Services.AddRazorPages();

builder.Services.AddSingleton<IStaticResourcesInfoProvider>(
	new StaticResourcesInfoProvider()
	.Add(SampleStaticPages.GetCollection())
	.Add(builder.Environment.GetWebRootCssResources(
		["css/**/*.css"]))
	.Add(builder.Environment.GetWebRootCssResources(
		["lib/bootstrap/**/*.css"],
		dontOptimize: true))
	.Add(builder.Environment.GetWebRootJsResources(
		["js/**/*.js"]))
	.Add(builder.Environment.GetWebRootJsResources(
		["lib/bootstrap/**/*.js",
		"lib/bootstrap/**/*.js.map",
		"lib/jquery*/**/*.js",
		"lib/jquery*/**/*.map"],
		dontOptimize: true))
	.Add(builder.Environment.GetWebRootBinResources(
		["favicon.ico"]))
	.Add(builder.Environment.GetWebRootBinResources(
		["lib/bootstrap/license",
		"lib/jquery*/license.*"],
		dontOptimize: true))
	);

// Use the "no-ssg" arg to omit static file generation
// during development (hot-reload, etc.)
var allowSSG = !args.HasOmitSsgArg();

var exitWhenDone = args.HasExitWhenDoneArg();

TimeSpan? regenInterval =
	!exitWhenDone &&
	TimeSpan.TryParse(builder.Configuration["AspNetStatic:RegenTimer"], out var ts)
	? ts : null;

#region app.UseStaticPageFallback()
#if ENABLE_STATIC_PAGE_FALLBACK

if (allowSSG)
{
	builder.Services.AddStaticPageFallback(
		cfg =>
		{
			cfg.AlwaysDefaultFile = false;
		});
}

#endif
#endregion


var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
}

app.UseHttpsRedirection();

#region app.UseStaticPageFallback()
#if ENABLE_STATIC_PAGE_FALLBACK

if (allowSSG)
{
	app.UseStaticPageFallback();
}

#endif
#endregion

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

if (allowSSG)
{
	app.GenerateStaticContent(
		app.Environment.WebRootPath,
		exitWhenDone: exitWhenDone,
		alwaysDefaultFile: false,
		dontUpdateLinks: false,
		dontOptimizeContent: false,
		regenerationInterval: regenInterval);
}

app.Run();

#if DEBUG
if (!exitWhenDone)
{
	Console.WriteLine("Press any Key to exit...");
	Console.ReadKey();
}
#endif

return 0;