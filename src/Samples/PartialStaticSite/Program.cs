#define ENABLE_STATIC_PAGE_FALLBACK

using AspNetStatic;
using AspNetStatic.Models;
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


var options = new GenerateStaticPagesOptions()
{
	DestinationRoot = GenerateStaticPagesOptions.ParseOptions(args, app.Environment.WebRootPath).DestinationRoot!,
	RegenerationInterval = regenInterval?.TotalMilliseconds,
};

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

app.GenerateStaticPages(options);

app.Run();

#if DEBUG
Console.WriteLine("Press any Key to exit...");
Console.ReadKey();
#endif
