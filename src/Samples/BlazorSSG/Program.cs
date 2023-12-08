using AspNetStatic;
using Sample.BlazorSsg;
using Sample.BlazorSSG.Components;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
	builder.WebHost.UseStaticWebAssets();
}

//----------------------------------------------------
// To serve only the generated static content, select 
// the Static-Site launchSettings configuration which 
// sets the CONTENT_MODE env var to enable this mode.
//----------------------------------------------------
var onlyServeStaticContent = "SHOW_STATIC_CONTENT_ONLY".Equals(
	Environment.GetEnvironmentVariable("CONTENT_MODE"),
	StringComparison.OrdinalIgnoreCase);

builder.Services.AddRazorComponents();

builder.Services.AddSingleton<IStaticPagesInfoProvider>(
	new StaticPagesInfoProvider(
		SampleStaticPages.GetCollection()));

//-----------------------------------------------------------
// The SSG launchSettings config adds the "ssg" command-line 
// argument used by the HasExitWhenDoneArg extension method.
//-----------------------------------------------------------
var runningInSsgMode = args.HasExitWhenDoneArg();


var app = builder.Build();

//-----------------------------------------------------------------
// These configurations are not useful when developing your pages.
//-----------------------------------------------------------------
//if (!app.Environment.IsDevelopment())
//{
//	app.UseExceptionHandler("/Error", createScopeForErrors: true);
//	app.UseHsts();
//}
//app.UseHttpsRedirection();
//-----------------------------------------------------------------

if (onlyServeStaticContent)
{
	app.UseDefaultFiles();
}

app.UseStaticFiles();

if (!onlyServeStaticContent)
{
	app.UseAntiforgery();

	app.MapRazorComponents<App>();

	if (runningInSsgMode)
	{
		app.GenerateStaticPages(
			app.Environment.WebRootPath,
			exitWhenDone: true);
	}
}

app.Run();
