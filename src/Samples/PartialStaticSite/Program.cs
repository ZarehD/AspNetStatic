#define ENABLE_STATIC_PAGE_FALLBACK

using AspNetStatic;
using PartialStaticSite;


var exitWhenDone = args.HasExitAfterStaticGenerationParameter();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(
	options =>
	{
		options.LowercaseUrls = true;
		options.LowercaseQueryStrings = true;
		options.AppendTrailingSlash = false;
	});

builder.Services.AddRazorPages();

builder.Services.AddSingleton<IStaticPagesInfoProvider, SampleStaticPagesInfoProvider>();


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

app.UseStaticPageFallback(
	cfg =>
	{
		cfg.AlwaysDefaultFile = false;
	});

#endif
#endregion

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.GenerateStaticPages(
	app.Environment.WebRootPath,
	exitWhenDone: exitWhenDone,
	alwaysDefautFile: false,
	dontUpdateLinks: false,
	dontOptimizeContent: false,
	regenerationInterval: regenInterval);

app.Run();

#if DEBUG
Console.WriteLine("Press any Key to exit...");
Console.ReadKey();
#endif
