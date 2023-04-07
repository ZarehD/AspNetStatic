#define ENABLE_STATIC_PAGE_FALLBACK

using AspNetStatic;
using Sample.PartialStaticSite;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(
	options =>
	{
		options.LowercaseUrls = true;
		options.LowercaseQueryStrings = true;
		options.AppendTrailingSlash = true;
	});

builder.Services.AddRazorPages();

builder.Services.AddSingleton<IStaticPagesInfoProvider, SampleStaticPagesInfoProvider>();


var app = builder.Build();

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
	args,
	alwaysDefautFile: false,
	dontUpdateLinks: false);

app.Run();
