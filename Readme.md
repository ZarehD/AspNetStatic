# AspNetStatic

![Platform Support: ASP.NET Core 6.0+](https://img.shields.io/static/v1?label=ASP.NET+Core&message=6.0%2b&color=blue&style=for-the-badge)
![License: Apache 2](https://img.shields.io/badge/license-Apache%202.0-blue?style=for-the-badge)
&nbsp;&nbsp;&nbsp;
[![Build-And-Test](https://github.com/ZarehD/AspNetStatic/actions/workflows/Build-and-Test.yml/badge.svg?branch=master)](https://github.com/ZarehD/AspNetStatic/actions/workflows/Build-and-Test.yml)

## Transform ASP.NET Core into a Static Site Generator

Okay, so you want to create a static website. After doing some research, you learn that all the cool kids are using tools like __Jekyll__, __Hugo__, __Gatsby__, __Statiq__, and others. 
But what you also learn is that all of these tools require you to learn an entirely new way of constructing sites and pages. 
And then it occurs to you, hey wait a minute, I already know how to use ASP.NET Core to create websites, so why do I need to learn & use a whole other stack just for SSG? Isn't there a better way that lets me use the tools and skills I already have?

Well, now there is!

### Create a static site from your ASP.NET Core generated content

AspNetStatic lets you generate a static website with the same ASP.NET Core tools you love and use every day. Just add this module and a bit of configuration, and BAM!, you have yourself a static site generator!

But wait, there's more!

AspNetStatic can also be used in a mixed mode configuration where some of the pages in your site are static html files (generated with the same \_layout & page layers that define the look & feel of the rest of your site), while others remain dynamically generated per request. See _Partial Static Site_ under _Scenarios_ section below.

### No Frameworks. No Engines. No Opinions!

Build your ASP.NET site the way you've always done. AspNetStatic doesn't have any opinions about how you should build your site.
AspNetStatic is not a framework. It's not a CMS. There's no blog engine. It has no templating system. 
AspNetStatic does just one thing, create static files for selected routes in your ASP.NET Core app.
That means you can use whatever framework, component, package, or architectural style you like. Want to use a blog engine like BlogEngine.NET? No problem. Want to use a CMS like Orchard or Umbraco? No problem. Want to create a documentation site using a markdown processor to render page content? No problem! 
AspNetStatic doesn't care; it will create optimized static files no matter how the content is produced by the server.


<br/>

## Great, So How Do I Use It?

It's a piece of cake!

1. Add the Nuget Package to your ASP.NET Core web app project
   ```
   dotnet add package AspNetStatic
   ```
1. Create and register an object that implements `IStaticPagesInfoProvider`
   - Create an instance of `StaticPagesInfoProvider`, or an object that derives from `StaticPagesInfoProviderBase`, or one that implements the interface directly
   - Populate the `Pages` collection to specify the routes for which to generate static pages
     - Set required `Route` property of each `PageInfo`
     - Set other `PageInfo` properties as appropriate
   - Set other `IStaticPagesInfoProvider` attributes as appropriate
   - Register the implementation class in the DI container
   ```c#
   builder.Services.AddSingleton<IStaticPagesInfoProvider>(
     new StaticPagesInfoProvider(
       new PageInfo[]
       {
         new("/") { ... },
         new("/privacy") { ... },
         new("/blog/posts/1") { OutFile = @"blog\post-1.html" },
         new("/blog/posts/2") { OutFile = @"blog\post-2-dark.html", Query = "?theme=dark" }
       }));
   ```
1. Add the AspNetStatic module
   ```c#
   ...
   app.MapRazorPages();
   ...
   app.GenerateStaticPages(app.Environment.WebRootPath);
   ...
   app.Run();
   ```
1. Run your web app
   ```
   dotnet run
   ```

Now, whenever you start your app, the static files will be (re-)created from their source Razor pages (or controller action views).

There are also options for exiting the app after generating static pages, or to periodically regenerate static pages while your app is running. See the __Scenarios__ section below for details.


<br/>

## Routes

Keep the following in mind when specifying routes in the `IStaticPagesInfoProvider.Pages` collection.

- Routes must exclude the site's base URI (e.g. __http:<span>://</span>localhost:5000__, __https<span>://www</span>.example.com__)
- As a rule, don't specify an 'index' page name; instead, opt for a route with a terminating slash (/ instead of /index).
- You can directly specify the pathname of the file to be generated for routes you add to the `Pages` collection (see `OutFile` property). The only requirement is that the specified path be relative to the destination root folder. If you do not specify a value for `OutFile`, the pathname for the generated file will be determined as demonstrated below.
- You can specify route parameters for routes you add to the `Pages` collection. The route parameters are treated as part of the route, and are used in constructing the output file pathname.
- You can specify a query string for routes you add to the `Pages` collection (see `Query` property). You can specify the same `Route` with different `Query` values, but you will need to specify a unique `OutFile` value for each instance of that route.
- You can skip content optimization<sup>1</sup> or choose a specific optimizer type for routes you add to the `Pages` collection (see `OptimizerType` property). The default optimizer type setting, `OptimizerType.Auto`, automatically selects the appropriate optimizer.
- You can set the encoding for content written to output files for routes you add to the `Pages` collection (see `OutputEncoding` property). Default is UTF8.

> 1: Content optimizer options apply only when content optimization is enabled. Please see the __Content Optimization__ section below for details.

### Routes vs. Generated Static Files

> Assumes the following:
>  - Destination root: "__C:\MySite__"
>  - OutFile: __null / empty / whitespace__

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


### Routes vs. Served Content (using fallback middleware)

> Assumes the following:
>  - OutFile: __null / empty / whitespace__

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


> #### The same rules apply when links in static files are updated to refer to other generated static files.


__IMPORTANT NOTE__: In ASP.NET Core, UrlHelper (and the asp-* tag helpers) generate link URLs based on the routing configuration of your app, so if you're using them, be sure to specify an appropriate value for `alwaysDefaultFile`, as shown below. (NOTE: Specify the same value if/when configuring the fallback middleware).
```c#
builder.Services.AddRouting(
  options =>
  { // default configuration in ASP.NET Core
    options.AppendTrailingSlash = false;   // generated links: / and /page
  });
...
app.GenerateStaticPages(
  alwaysDefaultFile: false);   // generated pages: /index.html and /page.index.html

--OR--

builder.Services.AddRouting(
  options =>
  {
    options.AppendTrailingSlash = true;   // generated links: / and /page/
  });
...
app.GenerateStaticPages(
  alwaysDefaultFile: true);   // generated pages: /index.html and /page/index.html
```


<br/>

## Scenarios

> #### In all scenarios, ensure that routes for static pages are unincumbered by authentication or authorization requirements.

### Standalone Static Site

In this scenario, you want to generate a completely static website (to host on Netlify or Azure/AWS storage, for instance). Once the static pages are generated, you will take the files in the destination folder (e.g. wwwroot), along with any .css, .js, and image files, and xcopy deploy them to your web host.

Sample Configuration 1:
  - Specify any accessible folder (e.g. _wwwroot_) as the destination-root for the generated static files.
  - Generate a default file only for routes ending with a slash.
  - Update href attribute value for \<a\> and \<area\> tags that refer to static pages (e.g. _/page_ to _/page.html_).
    ```c#
    app.GenerateStaticPages(
      app.Environment.WebRoot,
      exitWhenDone: true,
      alwaysDefaultFile: false,
      dontUpdateLinks: false);
    ```

Sample Configuration 2:
  - Specify any accessible folder (e.g. _wwwroot_) as the destination-root for the generated static files.
  - Generate a default file for all routes (e.g. _/page_ and _/page/_ to _/page/index.html_).
  - Don't update href attribute value for \<a\> and \<area\> tags that refer to static pages.
  - Use your web server's redirect or url-rewrite module to re-route requests (e.g. _/page/_ or _/page/index_ to _/page/index.html_).
    ```c#
    // true when app is executed with one of the following args...
    //  dotnet run -- static-only
    //  dotnet run -- exit-when-done
    var exitWhenDone = args.HasExitWhenDoneArg();

    app.GenerateStaticPages(
      @"C:\path\to\destination\root\folder",
      exitWhenDone: exitWhenDone,
      alwaysDefaultFile: true,
      dontUpdateLinks: true);
    ```

If you want to omit static-file generation while you're still developing the site (to save CPU cycles?), you could configure a profile in _launchSettings.json_ and surround the `GenerateStaticPages()` call with an IF gate.
```
"profiles": {
  "GenStaticPages": {
      "commandName": "Project",
      "commandLineArgs": "exit-when-done",
      "launchBrowser": false,
      "applicationUrl": "https://localhost:5000",
  }
}
```
```c#
if (args.HasExitWhenDoneArg())
{
  app.GenerateStaticPages(
    @"path\to\destination\root\folder",
    exitWhenDone: true,
  );
}
```


### Partial Static Site

In this scenario, you want some of the pages in your ASP.NET Core app to be static, but still want other routes to be served as dynamic content per request (pages/views and JSON API's). When the app runs, static (.html) files will be generated for routes you specify. The website will then serve these static files for the specified routes, and dynamic content, as usual, for others.

> While static files are being generated, requests for which the static file hasn't yet been generated will be served as dynamic content using the source (.cshtml) page. Once the static file has been generated, it will be used to satisfy requests.

The configuration options are the same as for a standalone static site, except the following:
 - The destination root folder must be `app.Environment.WebRoot`.
 - You must do one of the following (can do both):
   - Use the AspNetStatic fallback middleware.
   - Allow links in generated static files to be updated.
 - Do not exit the app after static files are generated (obviously, right?)
 
Like this:
```c#
...
builder.Services.AddStaticPageFallback();
...
app.UseStaticPageFallback();     // re-route to the static file
app.UseStaticFiles();
...
app.UseRouting();
...
app.Map...();
...
app.GenerateStaticPages(
  app.Environment.WebRoot,       // must specify wwwroot
  exitWhenDone: false,           // don't exit after generating static files
  alwaysDefaultFile: true/false,
  dontUpdateLinks: false);       // update links so they refer to the static file
...
app.Run();
```

> #### The fallback middleware only re-routes requests for routes that are specified in the `Pages` collection, and only if the static file exists.


#### Periodic Regeneration

If the data used in the content of static files changes while the app is running, you can configure periodic regeneration by specifying a value for the `regenerationInterval` parameter in the `GenerateStaticPages()` call. This will result in static files being generated when the app starts, and then periodically based on the specified interval.
```c#
app.GenerateStaticPages(
  ...
  regenerationInterval: TimeSpan.FromHours(2) // re-generate static files every 2 hours
);
```


<br/>

## Content Optimization

AspNetStatic automatically minifies HTML content (and any embedded CSS or Javascript) in generated static files; configuration is not required.
To disable this feature, specify `true` for the `dontOptimizeContent` parameter:
```c#
app.GenerateStaticPages(
  ...
  dontOptimizeContent: true);
```

### Configuration

To override the default minification settings used by AspNetStatic, register the appropriate objects in the DI container, as shown below.

> AspNetStatic uses the excellent WebMarkupMin package to implement the minification feature. For details about the configuration settings, please consult the WebMarkupMin [documentation](https://github.com/Taritsyn/WebMarkupMin/wiki/).

Content optimization can be customized in one of two ways:
 1. Create and register an object that implements `IOptimizerSelector`. In addition to specifying custom optimizer configurations, this option allows you to implement your own custom logic for selecting the optimizer to use for a page.
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
