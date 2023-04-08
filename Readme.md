# AspNetStatic

![Platform Support: ASP.NET Core 6.0+](https://img.shields.io/static/v1?label=ASP.NET+Core&message=6.0%2b&color=blue&style=for-the-badge)
![License: Apache 2](https://img.shields.io/badge/license-Apache%202.0-blue?style=for-the-badge)


## Transforms ASP.NET Core into a Static Site Generator


Okay, so you want to create a static website. After doing some research, you learn that all the cool kids are using tools like __Jekyll__, __Hugo__, __Gatsby__, __Statiq__, and others. 
But what you also learn is that all of these tools require you to learn an entirely new way of constructing sites and pages. 
And then it occurs to you, hey wait a minute, I already know how to use ASP.NET Core to create websites, so why oh why do I have to learn a whole new stack just for this? Isn't there a better way that lets me use the tools and skills I already know?

Well, now there is!

AspNetStatic lets you generate a static website with the same ASP.NET Core tools you love and use every day. Just add this module and a bit of configuration, and BAM!, you have yourself a static site generator!

But wait, there's more!

AspNetStatic can also be used in a mixed mode configuration where some of the pages in your site are static html files (generated with the same \_layout & page layers that define the look & feel of the rest of your site), while others remain dynamically generated per request. See _Partial Static Site_ under _Senarios_ section below.

### No Frameworks. No Engines. No Opinions!

AspNetStatic is not a framework. It's not a CMS. There's no blog engine. It has no templating system. AspNetStatic does just one thing (well, two, if you count the fallback middleware): create static HTML files for selected routes in your ASP.NET Core app.
That means you can use whatever framework, component, or package (or architectural style) you want in your app. Want to use a blog engine like BlogEngine.NET? No problem. Want to use a CMS like Orchard or Umbraco? No problem. Want to create a documentation site that uses a markdown processor to render page content? No problem! AspNetStatic doesn't care; it will create static files no matter how the content was produced.


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
      ```c#
      builder.Services.AddSingleton<IStaticPagesInfoProvider, MyStaticPagesInfoProvider>();
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

Now, whenever you start your app, the static files will be (re-)created from their source Razor pages (or controller action views).


<br/>

## Routes

Keep the follwing in mind when specifying routes in the `IStaticPagesInfoProvider.Pages` collection.

- Routes must exclude the site's base URI (e.g. __http:<span>://</span>localhost:5000__, __https<span>://www</span>.mysite.com__)
- As a rule, don't specify an 'index' page name; instead, opt for a route with a terminating slash (/ instead of /index).
- You can directly specify the pathname of the file to be generated for routes you add to the `Pages` collection (see `OutFilePathname` property). The only requirement is that the specified path be relative to the destination root folder. If you do not specify a value for `OutFilePathname`, the pathname for the generated file will be determined as demonstrated below.
- You can specify a query string (or route parameters) for routes you add to the `Pages` collection (see `QueryString` property). You can specify the same `Route` with different `QueryString` values, but be sure to specify a unique `OutFilePathname` value for each instance of that route.


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


__IMPORTANT NOTE__: In ASP.NET Core, UrlHelper (and the asp-* tag helpers) generate link URLs based on the routing configuration of your app, so if you're using them, be sure to specify an appropriate value for `alwaysDefaultFile`, as shown below.
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

## Senarios

> #### In all senarios, ensure that routes for static pages are unincumbered by authentication or authorization requirements.

### Standalone Static Site

In this senario, you want to generate a completely static website (to host on Netlify or Azure/AWS storage, for instance). Once the static pages are generated, you will take the files in the destination folder (e.g. wwwroot), along with any .css, .js, and image files, and xcopy deploy them to your web host.

Sample Configuration 1:
  - Specify any accessible folder (e.g. _wwwroot_) as the destination-root for the generated static files.
  - Generate a default file only for routes ending with a slash.
  - Update href attribute value for \<a\> and \<area\> tags that refer to static pages (e.g. _/page_ to _/page.html_).
    ```c#
    app.GenerateStaticPages(
      app.Environment.WebRoot,
      exitWhenDone: true,
      alwaysDefautFile: false,
      dontUpdateLinks: false);
    ```

Sample Configuration 2:
  - Specify any accessible folder (e.g. _wwwroot_) as the destination-root for the generated static files.
  - Generate a default file for all routes (e.g. _/page_ and _/page/_ to _/page/index.html_).
  - Don't update href attribute value for \<a\> and \<area\> tags that refer to static pages.
  - Use your web server's redirect or url-rewrite module to re-route requests (e.g. _/page/_ or _/page/index_ to _/page/index.html_).
    ```c#
    app.GenerateStaticPages(
      "C:\path\to\destination\root\folder",
      commandLineArgs: args, // exit when done if contains 'static-only' parameter
      alwaysDefautFile: true,
      dontUpdateLinks: true);
    ```

If you want to omit static-file generation while you're still developing the site (to save CPU cycles?), you could configure a StaticOnly profile in _launchSettings.json_ (as shown earlier) and surround the `app.GenerateStaticPages()` call with an IF gate in order to switch between static-page-generation and normal page-serving modes.
```c#
if (args.HasExitAfterStaticGenerationParameter())
{
  app.GenerateStaticPages(
    "path\to\destination\root\folder",
    exitWhenDone: true,
  );
}
```


### Partial Static Site

In this senario, you want some of the pages in your ASP.NET Core app to be static, but still want other routes to be served as dynamic content per request (pages/views and JSON API's). When the app runs, static (.html) files will be generated for routes you specify. The website will then serve these static files for the specified routes, and dynamic content, as usual, for others.

> While static files are being generated, requests for which the static file hasn't yet been generated will be served as dynamic content using the source (.cshtml) page. Once the static file has been generated, it will be used to statisfy requests.

The configuration options are the same as for a standalone static site, except the following:
 - The destination root folder must be `app.Environment.WebRoot`.
 - You must do one of the following (can do both):
   - Use the AspNetStatic fallback middleware.
   - Allow links in generaed static files to be updated.
 - Do not exit the app after static files are generated (obviously, right?)
 
Like this:
```c#
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
  alwaysDefautFile: true/false,
  dontUpdateLinks: false);       // update links so they refer to the static file
...
app.Run();
```

> #### The fallback middleware only re-routes requests for routes that are specified in the `Pages` collection, and only if the static file exists.


<br/>

## Content Optimization

AspNetStatic automatically minifies HTML content (and any embedded CSS or Javascript) in generated static files.
To disable this feature, specify `true` for the `dontOptimizeContent` parameter:
```c#
app.GenerateStaticPages(
  ...
  dontOptimizeContent: true);
```

### Configuration

To customize the minification settings used by AspNetStatic, register the appropriate objects in the DI container, as shown below.

> AspNetStatic uses the excellent WebMarkupMin package to implement the minification feature. For details about the configuration settings, please consult the WebMarkupMin [documentation](https://github.com/Taritsyn/WebMarkupMin/wiki/).

- __HTML__: To configure the HTML minifier, register a configured instance of `HtmlMinificationSettings`:
  ```c#
  using WebMarkupMin.Core;
  builder.Services.AddSingleton(
    sp => new HtmlMinificationSettings()
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
