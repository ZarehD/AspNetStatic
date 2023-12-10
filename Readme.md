# AspNetStatic

![Platform Support: ASP.NET Core 6.0+](https://img.shields.io/static/v1?label=ASP.NET+Core&message=6.0%20-%208.0&color=blue&style=for-the-badge)
![License: Apache 2](https://img.shields.io/badge/license-Apache%202.0-blue?style=for-the-badge)
&nbsp;&nbsp;&nbsp;
[![Build-And-Test](https://github.com/ZarehD/AspNetStatic/actions/workflows/Build-and-Test.yml/badge.svg?branch=master)](https://github.com/ZarehD/AspNetStatic/actions/workflows/Build-and-Test.yml)

## Transform ASP.NET Core into a Static Site Generator

Okay, so you want to create a static website. After doing some research, you learn that all the cool kids are using tools like __Jekyll__, __Hugo__, __Gatsby__, or __Statiq__. 
But what you also learn is that all of these tools require you to learn an entirely new way of constructing sites and pages. 
And then it occurs to you, I already know how to use ASP.NET Core to create websites, so why do I need to learn & use a whole other stack just for SSG? Isn't there a better way that lets me use the tools and skills I already have?

Well, now there is!

### Create a static site from your ASP.NET Core generated content

AspNetStatic lets you generate a static website with the same ASP.NET Core tools you love and use every day. Just add this module and a bit of configuration, and BAM!, you have yourself a static site generator.

But wait, there's more!

AspNetStatic can also be used in a mixed mode configuration where some of the pages in your site are static html files (generated with the same \_layout & page layers that define the look & feel of the rest of your site), while others remain dynamically generated per request. See _Partial Static Site_ under _Scenarios_ section below.

Oh, and one more thing, AspNetStatic also works with Blazor websites.

> Blazor support requires the new SSR capability in ASP.NET Core 8.0.

> Blazor pages must not rely on any client-side (JS, WASM) behavior for rendering, or behaviors like showing a placeholder (e.g. a spinner) before rendering the actual content. Server mode (SignalR) **may** be okay, but I haven't tested it. Basically, as long as the content has completed rendering by the time **AspNetStatic** receives it (i.e. HttpClient response), you're golden.

### No Frameworks. No Engines. No Opinions!

Build your ASP.NET site the way you've always done. AspNetStatic doesn't have any opinions about how you should build your server-rendered site.
AspNetStatic is not a framework. It's not a CMS. There's no blog engine. It has no templating system. 
AspNetStatic does just one thing, create static files for selected routes in your ASP.NET Core app.
That means you can use whatever framework, component, package, or architectural style you like. Want to use a blog engine? No problem. Want to use a CMS? No problem. Want to create a documentation site using a markdown processor to render page content? No problem! 
AspNetStatic doesn't care; it will create optimized static files no matter how the content is produced by the server.


<br/>

## Great. So how do I use it?

It's a piece of cake.

1. Add the Nuget Package to your ASP.NET Core web app project
   ```
   dotnet add package AspNetStatic
   ```
1. Specify the routes for which you want static files to be generated
   - Create an instance of `StaticResourcesInfoProvider` (*or an object that derives from `StaticResourcesInfoProviderBase` or implements the `IStaticResourcesInfoProvider` interface*)
   - Populate the `PageResources` and/or `OtherResources` collections
     - Set required `Route` property of each item
     - Set other properties as appropriate
   - Set other `IStaticResourcesInfoProvider` attributes as appropriate
   - Register it in the DI container
   ```c#
   builder.Services.AddSingleton<IStaticResourcesInfoProvider>(
     new StaticResourcesInfoProvider(
       new PageResource[]
       {
         new("/"),
         new("/privacy"),
         new("/blog/articles/posts/1") { OutFile = "blog/post-1.html" },
         new("/blog/articles/posts/2") { OutFile = "blog/post-2-dark.html", Query = "?theme=dark" }
       }, null));
   ```
1. Add a call to the AspNetStatic module in the app startup
   ```c#
   ...
   app.MapRazorPages();
   ...
   app.GenerateStaticContent(@"C:\SSG-Output-Folder");
   app.Run();
   ```
1. Run your app
   ```
   dotnet run
   ```

### AspNetStatic is the EASY button for doing SSG with ASP.NET

You can use AspNetStatic in traditional SSG mode (*generate files and exit the app*), or in a 'partial-static site' mode. There is also an option to periodically regenerate the static content while your app is running. See the __Scenarios__ section below for details.


<br/>

## Routes

Keep the following in mind when specifying routes in the `IStaticResourcesInfoProvider.PageResources` collection.

- Routes must exclude the site's base URI (e.g. __http:<span>://</span>localhost:5000__, __https<span>://www</span>.example.com__)
- As a rule, don't specify an 'index' page name; instead, opt for a route with a terminating slash (/ instead of /index).
- You can directly specify the pathname of the file to be generated for routes you add to the `PageResources` collection (see `OutFile` property). The only requirement is that the specified path be relative to the destination root folder. If you do not specify a value for `OutFile`, the pathname for the generated file will be determined as demonstrated below.
- You can specify route parameters for routes you add to the `PageResources` collection. The route parameters are treated as part of the route, and are used in constructing the output file pathname.
- You can specify a query string for routes you add to the `PageResources` collection (see `Query` property). You can specify the same `Route` with different `Query` values, but you will need to specify a unique `OutFile` value for each instance of that route.
- You can skip content optimization<sup>1</sup> or choose a specific optimizer type for routes you add to the `PageResources` collection (see `OptimizerType` property). The default optimizer type setting, `OptimizerType.Auto`, automatically selects the appropriate optimizer.
- You can set the encoding for content written to output files for routes you add to the `PageResources` collection (see `OutputEncoding` property). Default is UTF8.

> NOTE: All of the above also applies to routes for CSS, JavaScript, and binary (e.g. image) files specified in the `OtherResources` collection property.

> 1: Content optimization options apply only when content optimization is enabled. Please see the __Content Optimization__ section below for details. Also note that no optimization is performed on binary resources (`BinResource` items).

### Routes vs. Generated Static Files

> Assumes the following:
>  - Resource Type: PageResource
>  - Destination root: "__C:\MySite__"
>  - OutFile: __null, empty, or whitespace__

Url<br/>(route + query) | Always Default<br/>false | Always Default<br/>true
---|---|---
/                    | C:\MySite\index.html                  | C:\MySite\index.html
/index               | C:\MySite\index.html                  | C:\MySite\index.html
/index/              | C:\MySite\index\index.html            | C:\MySite\index\index.html
/page                | C:\MySite\page.html                   | C:\MySite\page\index.html
/page/               | C:\MySite\page\index.html             | C:\MySite\page\index.html
/page/123            | C:\MySite\page\123.html               | C:\MySite\page\123\index.html
/page/123/           | C:\MySite\page\123\index.html         | C:\MySite\page\123\index.html
/page/123?p1=v1      | C:\MySite\page\123.html               | C:\MySite\page\123\index.html
/page/123/?p1=v1     | C:\MySite\page\123\index.html         | C:\MySite\page\123\index.html
/blog/articles/      | C:\MySite\blog\articles/index.html    | C:\MySite\blog\articles\index.html
/blog/articles/post1 | C:\MySite\blog\articles\post1.html | C:\MySite\blog\articles\post1\index.html

> Assumes the following:
>  - Resource Type: __CssResource__, __JsResource__, or __BinResource__
>  - Destination root: "__C:\MySite__"
>  - OutFile: __null, empty, or whitespace__
>  - __AlwaysDefaultFile__ setting not applicable.

Url<br/>(route + query) | Generated File
---|---
/file.css         | C:\MySite\file.css
/folder/file.css  | C:\MySite\folder\file.css
/file.css?v=123   | C:\MySite\file.css
/file             | C:\MySite\file.css (__CssResource__)
/file/            | C:\MySite\file.css (__CssResource__)
/file.js          | C:\MySite\file.css
/folder/file.js   | C:\MySite\folder\file.js
/file.js?v=123    | C:\MySite\file.js
/file             | C:\MySite\file.js (__JsResource__)
/file/            | C:\MySite\file.js (__JsResource__)
/file.png         | C:\MySite\file.png
/folder/file.png  | C:\MySite\folder\file.png
/file.png?v=123   | C:\MySite\file.png
/file             | C:\MySite\file.bin (__BinResource__)
/file/            | C:\MySite\file.bin (__BinResource__)


### Fallback Middleware: Routes vs. Served Content

> Assumes the following:
>  - OutFile: __null, empty, or whitespace__
>  - Applicable only to __PageResource__ items.

Url<br/>(route + query) | Is Static Route: false<br/><br/> | Is Static Route: true<br/>Always Default: false | Is Static Route: true<br/>Always Default: true
---|---|---|---
/                    | /index.cshtml                  | /index.html                  | /index.html
/index               | /index.cshtml                  | /index.html                  | /index.html
/index/              | /index/index.cshtml            | /index/index.html            | /index/index.html
/page                | /page.cshtml                   | /page.html                   | /page/index.html
/page/               | /page/index.cshtml             | /page/index.html             | /page/index.html
/page/123            | /page.cshtml                   | /page/123.html               | /page/123/index.html
/page/123/           | /page.cshtml                   | /page/123/index.html         | /page/123/index.html
/page/123?p1=v1      | /page.cshtml                   | /page/123.html               | /page/123/index.html
/page/123/?p1=v1     | /page.cshtml                   | /page/123/index.html         | /page/123/index.html
/blog/articles/      | /blog/articles/index.cshtml    | /blog/articles/index.html    | /blog/articles/index.html
/blog/articles/post1 | /blog/articles/post1.cshtml | /blog/articles/post1.html | /blog/articles/post1/index..html


> #### The same rules apply when links in static files are updated to refer to other generated static pages.


__IMPORTANT NOTE__: In ASP.NET Core, UrlHelper (and the asp-* tag helpers) generate link URIs based on the routing configuration of your app, so if you're using them, be sure to specify an appropriate value for `alwaysDefaultFile`, as shown below. (NOTE: Specify the same value if/when configuring the fallback middleware).
```c#
// Sample routes: /, /index, and /page
//-------------------------------------

// generated links: /, /index, and /page
builder.Services.AddRouting(
  options =>
  { // default configuration in ASP.NET Core
    options.AppendTrailingSlash = false;
  });
...
// fallback static pages: /index.html, /index.html, and /page.html
builder.Services.AddStaticPageFallback(
  cfg =>
  {
    cfg.AlwaysDefaultFile = false;
  });
...
// generated static pages: /index.html, /index.html, and /page.html
app.GenerateStaticContent(
  alwaysDefaultFile: false);

-- OR --

// generated links: /, /index/, and /page/
builder.Services.AddRouting(
  options =>
  {
    options.AppendTrailingSlash = true;
  });
...
// fallback static pages: /index.html, /index/index.html, and /page/index.html
builder.Services.AddStaticPageFallback(
  cfg =>
  {
    cfg.AlwaysDefaultFile = true;
  });
...
// generated static pages: /index.html, /index/index.html, and /page/index.html
app.GenerateStaticContent(
  alwaysDefaultFile: true);
```


<br/>

## Scenarios

> #### In all scenarios, ensure that routes for static content are unincumbered by authentication or authorization requirements.

### Static Site Generation (Standalone SSG)

In this scenario, you want to generate a completely static website (to host on Netlify or Azure/AWS storage, for instance). Once the static pages are generated, you will take the files in the destination folder and xcopy deploy them to your web host.

__Sample Configuration 1__:
  - Specify any accessible folder as the destination-root for the generated static files.
  - Generate a default file only for routes ending with a slash.
  - Update the `href` attribute for __\<a\>__ and __\<area\>__ tags that refer to static pages (e.g. _/page_ to _/page.html_).
    ```c#
    app.GenerateStaticContent(
      "../SSG_Output",
      exitWhenDone: true,
      alwaysDefaultFile: false,
      dontUpdateLinks: false);
    ```

__Sample Configuration 2__:
  - Specify any accessible folder as the destination-root for the generated static files.
  - Generate a default file for all routes (e.g. _/page_ and _/page/_ to _/page/index.html_).
  - Don't update the `href` attribute for __\<a\>__ and __\<area\>__ tags that refer to static pages.
  - Use your web server's features to re-route requests (e.g. _/page/_ or _/page/index_ to _/page/index.html_).
    ```c#
    // true when app is executed with one of the marker args, such as SSG.
    //  dotnet run -- ssg
    var exitWhenDone = args.HasExitWhenDoneArg();

    app.GenerateStaticContent(
      @"C:\path\to\destination\root\folder",
      exitWhenDone: exitWhenDone,
      alwaysDefaultFile: true,
      dontUpdateLinks: true);
    ```

If you want to omit static-file generation while you're still developing the site, you can configure a _launchSettings_ profile for SSG mode operation. To enable this,  you would surround the `GenerateStaticContent()` call with an IF gate.
```
"profiles": {
  "SSG": {
      "commandName": "Project",
      "commandLineArgs": "ssg",
      "launchBrowser": false,
      "applicationUrl": "https://localhost:5000",
  }
}
```
Then, in the startup code (_Program.cs_)
```c#
if (args.HasExitWhenDoneArg())
{
  app.GenerateStaticContent(
    @"path\to\destination\root\folder",
    exitWhenDone: true,
  );
}
```
Now you can use the SSG profile to launch your app in SSG mode (to generate static content, then exit), and a differrent launch profile while you're in development mode, editing the site content. (*The __BlazorSSG__ sample demonstrates this approach*.)



### Partial Static Site

In this scenario, you want some of the pages in your ASP.NET Core app to be static, but still want other routes to be served as dynamic content per request (e.g. pages/views, JSON API's, etc.). When the app runs, static (.html) files will be generated for routes you specify. The website will then serve these static files for the specified routes, and dynamic content (as usual) for others.

> While static files are being generated, requests to routes for which a static file has not yet been generated will be served as dynamicly generated content (using the source .cshtml page). Once the static file for that route has been generated, it will be used to satisfy subsequent requests.

The configuration options are generally the same as for a standalone static site, except the following differences:
 - The destination root folder must be `app.Environment.WebRoot` (i.e. wwwroot).
 - You must do one of the following (can do both):
   - Use the AspNetStatic fallback middleware.
   - Allow links in generated static files to be updated (`href` of __\<a\>__ and __\<area\>__ tags).
 - Do not exit the app after static files are generated (obviously, right?)
 
Like this:
```c#
...
builder.Services.AddStaticPageFallback();
...
app.UseStaticPageFallback();     // re-route to the static file (page resources only)
app.UseStaticFiles();
...
app.UseRouting();
...
app.Map...();
...
app.GenerateStaticContent(
  app.Environment.WebRoot,       // must specify wwwroot
  exitWhenDone: false,           // don't exit after generating static files
  alwaysDefaultFile: true/false,
  dontUpdateLinks: false);       // update links so they refer to static files
...
app.Run();
```

> #### NOTE: The fallback middleware only re-routes requests for routes that match entries in the `PageResources` collection, and only if a generated static file exists for that route.


#### Periodic Regeneration

If the data used in the content of static files changes while the app is running, you can configure periodic regeneration by specifying a value for the `regenerationInterval` parameter in the `GenerateStaticContent()` call. This will result in static files being generated when the app starts, and then periodically based on the specified interval.
```c#
app.GenerateStaticContent(
  ...
  exitWhenDone: false,
  regenerationInterval: TimeSpan.FromHours(2) // re-generate static files every 2 hours
);
```


<br/>

## Content Optimization

AspNetStatic automatically minifies HTML content (and any embedded CSS or Javascript) in generated static files; configuration is not required.
To disable this feature, however, you can specify `true` for the `dontOptimizeContent` parameter.
```c#
app.GenerateStaticContent(
  ...
  dontOptimizeContent: true);
```

> Content optimization does not apply to binary resource types (`BinResource` entries), but is enabled by default (`OptimizerType.Auto`) for all other resource types (`PageResource`, `CssResource`, and `JsResource` entries).

### Configuration

To override the default minification settings used by AspNetStatic, register the appropriate objects in the DI container, as shown below.

> AspNetStatic uses the excellent WebMarkupMin package to implement the minification feature. For details about the configuration settings, please consult the WebMarkupMin [documentation](https://github.com/Taritsyn/WebMarkupMin/wiki/).

Content optimization can be customized in one of two ways:
 1. Create and register an object that implements `IOptimizerSelector`. In addition to specifying custom optimizer configurations, this option allows you to implement your own custom logic for selecting the optimizer to use for a resource.
    ```c#
    public class MyOptimizerSelector : IOptimizerSelector { ... }
    ...
    builder.Services.AddSingleton(sp => new MyOptimizerSelector( ... ));
    ```

 1. Create and register individual settings objects which internally feed into a default `IOptimizerSelector` implementation.
    - __HTML__: To configure the HTML minifier, register a configured instance of `HtmlMinificationSettings`:
      ```c#
      using WebMarkupMin.Core;
      builder.Services.AddSingleton(
        sp => new HtmlMinificationSettings()
        {
          ...
        });
      ```

    - __XHTML__: To configure the XHTML minifier, register a configured instance of `XhtmlMinificationSettings`:
      ```c#
      using WebMarkupMin.Core;
      builder.Services.AddSingleton(
        sp => new XhtmlMinificationSettings()
        {
          ...
        });
      ```

    - __XML__: To configure the XML minifier, register a configured instance of `XmlMinificationSettings`:
      ```c#
      using WebMarkupMin.Core;
      builder.Services.AddSingleton(
        sp => new XmlMinificationSettings()
        {
          ...
        });
      ```

    - __CSS__: To configure the CSS minifier, register an object that implements the `ICssMinifier` interface:
      ```c#
      using WebMarkupMin.Core;
      builder.Services.AddSingleton<ICssMinifier>(
        sp => new YuiCssMinifier(...));
      ```

    - __Javascript__: To configure the Javascript minifier, register an object that implements the `IJsMinifier` interface:
      ```c#
      using WebMarkupMin.Core;
      builder.Services.AddSingleton<IJsMinifier>(
        sp => new YuiJsMinifier(...));
      ```

<br/>

## License

[Apache 2.0](https://github.com/ZarehD/AspNetStatic/blob/master/LICENSE)

<br/>

If you like this project, or find it useful, please give it a star. Thank you.
