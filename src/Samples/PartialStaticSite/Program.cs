#define ENABLE_STATIC_PAGE_FALLBACK

using AspNetStatic;
using AspNetStaticContrib;
using AspNetStaticContrib.AspNetStatic;
using AspNetStaticContrib.Stekeblad;
using PartialStaticSite;

static void AddExampleSiteAspNetStatic(WebApplicationBuilder builder, int example)
{
	switch (example)
	{
		case 1:
			// Example 1:
			// Manually specify a list of pages and resources that should
			// be included in the static site generation

			builder.Services.AddAspNetStatic();
			builder.Services.AddSingleton<IStaticResourcesInfoProvider>(
				new StaticResourcesInfoProvider(SampleStaticPages.GetCollection())
				.Add(builder.Environment.GetWebRootCssResources(["**/site.css", "**/*.min.css"]))
				.Add(builder.Environment.GetWebRootJsResources())
				.Add(builder.Environment.GetWebRootBinResources(["**/*.ico"]))
			);
			break;
		case 2:
			// Example 2:
			// Manually specify assets in wwwroot that should
			// be included in the static site generation and register
			// a second IStaticResourcesInfoProvider to discover Razor Pages at runtime

			builder.Services.AddAspNetStatic()
				.AddAspNetStaticContrib(); // Contrib required for ActionDescriptorPageResourceInfoProvider

			builder.Services.AddSingleton<IStaticResourcesInfoProvider>(
				new StaticResourcesInfoProvider()
				.Add(builder.Environment.GetWebRootCssResources(["**/site.css", "**/*.min.css"]))
				.Add(builder.Environment.GetWebRootJsResources())
				.Add(builder.Environment.GetWebRootBinResources(["**/*.ico"]))
			);

			builder.Services.AddSingleton<IStaticResourcesInfoProvider>(
			new ActionDescriptorPageResourceInfoProvider()
			);
			break;
	}
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(
	options =>
	{
		options.LowercaseUrls = true;
		options.LowercaseQueryStrings = true;
		options.AppendTrailingSlash = false;
	});

builder.Services.AddRazorPages();

AddExampleSiteAspNetStatic(builder, example: 2);

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