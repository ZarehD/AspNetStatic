#define ENABLE_STATIC_PAGE_FALLBACK

using AspNetStatic;
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
	new StaticResourcesInfoProvider(SampleStaticPages.GetCollection()));


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

var exitWhenDone = args.HasExitWhenDoneArg();
TimeSpan? regenInterval =
	!exitWhenDone &&
	TimeSpan.TryParse(app.Configuration["AspNetStatic:RegenTimer"], out var ts)
	? ts : null;

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

app.GenerateStaticContent(
	app.Environment.WebRootPath,
	exitWhenDone: exitWhenDone,
	alwaysDefaultFile: false,
	dontUpdateLinks: false,
	dontOptimizeContent: false,
	regenerationInterval: regenInterval);

app.Run();

#if DEBUG
if (!exitWhenDone)
{
	Console.WriteLine("Press any Key to exit...");
	Console.ReadKey();
}
#endif
