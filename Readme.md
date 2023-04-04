# AspNetStatic

![Platform Support: ASP.NET Core 6.0+](https://img.shields.io/static/v1?label=.NET+Platform+/+ASP.NET+Core&message=6.0%2b&color=blue&style=for-the-badge)
![License: Apache 2](https://img.shields.io/badge/license-Apache%202-blue?style=for-the-badge)


## Transforms ASP.NET Core into a static site generator.


Okay, so you want to create a static website, and after doing a bit of research, you learn that all the cool kids are using tools like __Jekyll__, __Hugo__, __Gatsby__, or __Statiq__ if you want to use .NET and C#. 
But what you also learn is that all of these tools require you to learn an entirely new way of constructing sites and pages.
And then it occurs to you, hey wait a minute, I already know how to use ASP.NET Core to create websites, so why oh why do I have to learn a whole new stack just for this? Isn't there a better way that lets me use the tools and skills I already know?

Well, now there is!

AspNetStatic lets you generate a static website with the same ASP.NET Core tools you love and use every day. Just add this module and a tiny bit of configuration, and BAM!, you have yourself a static site generator!

But wait, there's more!

AspNetStatic can also be used in a mixed mode configuration where some of the pages in your site are static html files (that use the same _layout & page layers that define the look & feel of the rest of your site), while others remain dynamically generated per request. See Partial Static Site under Senarios section below.


<br/>

## Great, So How Do I Use It?

It's a peace of cake!

1. Add the Nuget Package to your ASP.NET Core web app project
   ```
   dotnet add package AspNetStatic
   ```
1. Create and register a class that implements `IStaticPagesInfoProvider`
	- Derrive from `StaticPagesInfoProviderBase` or implement the interface directly
	- Populate the `Pages` collection to specify the routes for which to generate static pages
	- Set other properties as appropriate
	- Register the class in the DI container
      ``` C#
      builder.Services.AddSingleton<IStaticPagesInfoProvider, MyStaticPagesInfoProvider>
      ```
1. Add the AspNetStatic module
   ``` C#
   ...
   app.MapRazorPages();
   ...
   app.GenerateStaticPages(args, app.Environment.WebRootPath);
   ...
   app.Run();
   ```
1. Run your web app
   ```
   dotnet run
   -OR-
   dotnet run -- static-only  // exits the app after generating static pages
   ```

   Via launchSettings.json:
   ```
   "StaticOnly": {
      "commandName": "Project",
      "commandLineArgs": "static-only",
      "launchBrowser": false,
      "applicationUrl": "https://localhost:5000",
   }
   ```

Now, whenever you start your app, your static pages will be regenerated to reflect any changes you may have made to their source Razor pages (or controller action views).


<br/>

## Routes

Keep the follwing in mind when specifying routes in the `IStaticPagesInfoProvider.Pages` collection.

- As a rule, don't specify an 'index' page name; instead, opt for a route with a terminating slash.
- You can directly specify the pathname of the file to be generated for routes you add to the `Pages` collection (see `OutFilePathname` property). The only requirement is that the specified path be relative to the destination root folder. If you do not specify a value for `OutFilePathname`, the pathname for the generated file will be determined as demonstrated below.
- You can specify a query string (or route parameters) for routes you add to the `Pages` collection (see `QueryString` property). You can specify the same `Route` with different `QueryString` values in order to vary the generated content, but be sure to specify a unique `OutFilePathname` value for each instance of that route.
- Routes can refer to any text-based, non-binary resource (.js or .css, for instance, but not image files). For such routes, always specify a value for `OutFilePathname`. Note that, currently, links in generated files are not updated for such routes if they appear in __\<link\>__ and __\<script\>__ tags (but will in __\<a\>__ and __\<area\>__ tags).


### Routes vs. Generated Static Files

> Assume destination root folder is "__C:\MySite__".

<br/>Route | Always Default<br/>false | Always Default<br/>true
---|---|---
/                       | C:\MySite\index.html                  | C:\MySite\index.html
/index                  | C:\MySite\index.html                  | C:\MySite\index.html
/index/                 | C:\MySite\index\index.html            | C:\MySite\index\index.html
/page                   | C:\MySite\page.html                   | C:\MySite\page\index.html
/page/                  | C:\MySite\page\index.html             | C:\MySite\page\index.html
/blog/articles/         | C:\MySite\blog\articles/index.html    | C:\MySite\blog\articles\index.html
/blog/articles/article1 | C:\MySite\blog\articles/article1.html | C:\MySite\blog\articles\article1/index.html



### Routes vs. Served Content (using fallback middleware)

Route<br/> | Is Static Route: false<br/><br/> | Is Static Route: true<br/>Always Default: false | Is Static Route: true<br/>Always Default: true
---|---|---|---
/                       | /index.cshtml                  | /index.html                  | /index.html
/index                  | /index.cshtml                  | /index.html                  | /index.html
/index/                 | /index/index.cshtml            | /index.html                  | /index/index.html
/page                   | /page.cshtml                   | /page.html                   | /page/index.html
/page/                  | /page/index.cshtml             | /page/index.html             | /page/index.html
/blog/articles/         | /blog/articles/index.cshtml    | /blog/articles/index.html    | /blog/articles/index.html
/blog/articles/article1 | /blog/articles/article1.cshtml | /blog/articles/article1.html | /blog/articles/article1/index..html


> #### The same rules apply when links in static files are updated to refer to other generated static files.


__IMPORTANT NOTE__: In ASP.NET Core, UrlHelper (and the asp-xxx tag helpers) generate link urls based on the routing configuration of your app, so be sure to specify an appropriate value for `alwaysDefaultFile`, as below.
``` C#
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

## Senarios

> #### In all senarios, ensure that routes for static pages are unincumbered by authentication or authorization requirements.

### Standalone Static Site

In this senario, you want to generate a completely static website (to host on Netlify or Azure/AWS storage, for instance). Once the static pages are generated, you will take the files in the destination folder (e.g. wwwroot), along with any .css, .js, and image files, and xcopy deploy them to your web host.

> #### NOTE: You can use our CLI or desktop app to generate a standalone site from ANY website.

Sample Configuration 1:
  - Specify any accessible folder (e.g. __wwwroot__) as the destination-root for the generated static files.
  - Generate a defailt file only for routes ending with a slash.
  - Update href attribute value for \<a\> and \<area\> tags that refer to static pages (from /page to /page.html).
    ``` C#
    app.GenerateStaticPages(
      args,
      app.Environment.WebRoot,
      alwaysDefautFile: false,
      dontUpdateLinks: false);
    ```

Sample Configuration 2:
  - Specify any accessible folder (e.g. __wwwroot__) as the destination-root for the generated static files.
  - Generate a defailt file for all routes (/page and /page/ to /page/index.html).
  - Update href attribute value for \<a\> and \<area\> tags that refer to static pages (from /page and /page/ to /page/index.html).
    ``` C#
    app.GenerateStaticPages(
      args,
      "C:\path\to\destination\root\folder",
      alwaysDefautFile: true,
      dontUpdateLinks: false);
    ```

> #### If you opt not to update links in the generated files (`dontUpdateLinks: true`), you will need to configure your web host to use a redirect or url-rewrite module so that pages referenced by links can be accessed (e.g. \<a href="/page" \> attempting to access "/page.html").


### Partial Static Site

In this senario, you want some of the pages in your ASP.NET Core app to be static, but still want other routes to be served as dynamic content per request (pages/views and JSON API's). When the app runs, static (.html) files will be generated for routes you specify. The website will then serve these static files for the specified routes, and dynamic content, as usual, for others.

> While static files are being generated, requests for which the static file hasn't yet been generated will be served as dynamic content using the source (.cshtml) page. Once the static file has been generated, it will be used to statisfy requests.

The configuration options are the same as for a standalone static site, except the following:
 - The destination root folder must be `app.Environment.WebRoot`.
 - You must do one of the following (can do both):
   - Use the AspNetStatic static-page fallback middleware.
   - Allow links in generaed static files to be updated.
 - Do not specify the static-only command-line parameter when running the app (obviously, right?)
 
Like this:
``` C#
...
app.UseStaticPageFallback();     // use fallback middleware to route to .html page
app.UseStaticFiles();
...
app.UseRouting();
...
app.Map...();
...
app.GenerateStaticPages(
  args,                          // no static-only parameter
  app.Environment.WebRoot,       // must specify wwwroot
  alwaysDefautFile: true/false,
  dontUpdateLinks: false);       // update links so they refer to the .html page
...
app.Run();
```


<br/>

## License

Apache 2.0
