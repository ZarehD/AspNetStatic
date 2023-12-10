using AspNetStatic;
using Sample.BlazorSsg;
using Sample.BlazorSSG.Components;

//----------------------------------------------------
// To serve only the generated static content, select 
// the Static-Site launchSettings configuration which 
// sets the CONTENT_MODE env var to enable this mode.
//----------------------------------------------------
var onlyServeStaticContent = "SHOW_STATIC_CONTENT_ONLY".Equals(
	Environment.GetEnvironmentVariable("CONTENT_MODE"),
	StringComparison.OrdinalIgnoreCase);

//-----------------------------------------------------------
// The SSG launchSettings config adds the "ssg" command-line 
// argument used by the HasExitWhenDoneArg extension method.
//-----------------------------------------------------------
var runningInSsgMode = args.HasExitWhenDoneArg();


var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
	builder.WebHost.UseStaticWebAssets();
}

builder.Services.AddRazorComponents();

if (!onlyServeStaticContent && runningInSsgMode)
{
	builder.Services.AddSingleton<IStaticResourcesInfoProvider>(
		StaticResourcesInfo.GetProvider());
}


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
		// NOTE: This folder name is specified in .gitIgnore
		// in order to avoid tracking the generated files.
		const string SSG_DEST_ROOT_FOLDER_NAME = "BlazorSsgOutput";

		var SsgOutputPath = Path.Combine(
			"../", SSG_DEST_ROOT_FOLDER_NAME);

		Directory.CreateDirectory(SsgOutputPath);

		app.GenerateStaticContent(
			SsgOutputPath,
			exitWhenDone: true);
	}
}

app.Run();
