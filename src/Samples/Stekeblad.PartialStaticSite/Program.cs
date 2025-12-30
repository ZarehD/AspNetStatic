#define ENABLE_STATIC_PAGE_FALLBACK

using AspNetStatic;
using AspNetStaticContrib.AspNetStatic;
using AspNetStaticContrib.Stekeblad;
using AspNetStaticContrib.Stekeblad.Extensions;
using AspNetStaticContrib.Stekeblad.ResourceLocators.ActionDescriptor;
using AspNetStaticContrib.Stekeblad.ResourceLocators.Sitemap;
using AspNetStaticContrib.Stekeblad.Options;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(
	options =>
	{
		options.LowercaseUrls = true;
		options.LowercaseQueryStrings = true;
		options.AppendTrailingSlash = false;
	});

builder.Services.AddRazorPages();

builder.Services.AddAspNetStaticContribStekeblad(opts =>
{
	opts.RegisterActionDescriptorResourceLocator = true;
	opts.RegisterSitemapResourceLocator = true;
});

static PageResource PageFactory(string route)
{
	return new PageResource(route)
	{
		OptimizationType = AspNetStatic.Optimizer.OptimizationType.None
	};
}

var actDescLocOpts = new ResourceLocatorOptions
{
	PageResourceFactory = PageFactory
};

LocatingStaticResourcesInfoProvider locatingProvider = new();

locatingProvider.AddActionDescriptorLocator(actDescLocOpts)
	.AddSitemapLocator()
	//.AddResourceLocator(new SitemapPageResourceLocator(new SitemapResourceLocatorOptions()))
	.Add(builder.Environment.GetWebRootCssResources(["**/site.css", "**/*.min.css"]))
	.Add(builder.Environment.GetWebRootJsResources())
	.Add(builder.Environment.GetWebRootBinResources(["**/*.ico"]));

builder.Services.AddSingleton<IStaticResourcesInfoProvider>(locatingProvider);

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
	var signal = new EventWaitHandle(false, EventResetMode.ManualReset);

	app.LocateStaticResources(signal);
	app.GenerateStaticContent(
		app.Environment.WebRootPath,
		exitWhenDone: exitWhenDone,
		alwaysDefaultFile: false,
		dontUpdateLinks: false,
		dontOptimizeContent: false,
		regenerationInterval: regenInterval,
		signalTimeoutSeconds: 60,
		processStartSignal: signal);
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