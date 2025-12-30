# AspNetStaticContrib.Stekeblad

AspNetStaticContrib.Stekeblad is an extension to [AspNetStatic](https://github.com/ZarehD/AspNetStatic/)
that adds support for "Resource Locators", a way to dynamically discover pages and other resources to be
included by the static site generator.

By default, AspNetStatic only offers direct support for defining resources in source or during app initialization.
If you want to generate a fully static site this means you need to maintain a list of resources to include.

This extension adds support for discovering pages and other resources after the app has completely started,
enabling you to write code with access to all registered services and automate building the resource list.

> [!Important]
> Resource locators will only be run once. Rediscovering content is not supported.
> That means that if you use periodic regeneration of static content then resource locators
> will not pick up on added or removed resources.

## How Do I Start Using Resource Locators?

This description assumes you are already using AspNetStatic or have some knowledge of it.

1. Instead of registering `StaticResourcesInfoProvider` or a custom type as the
implementation of `IStaticResourcesInfoProvider` you need to use or derive from
`LocatingStaticResourcesInfoProvider`.

1. Similar to how you add resources, call `AddResourceLocator` to add a resource locator.
The built-in once also have extension methods for convenience. List of built-in
resource locators and how to create your own is described further down.

1. The built-in resource locators need one or more services to be registered,
if you use any of them you need to call and configure
`builder.AddAspNetStaticContribStekeblad(...)`

1. Before calling `app.GenerateStaticContent(...)` you need to add a call to
`app.LocateStaticResources(EventWaitHandle)`. This is required for resource locators to be executed.

> [!Important]
> The `EventWaitHandle` instance provided to `LocateStaticResources` must also be provided to
> `GenerateStaticContent`. This ensures the static generator won't start until all resources
> has been discovered. Forgetting this results in a large risk of concurrency-related exceptions.

> [!Note]
> `GenerateStaticContent` also has the argument `signalTimeoutSeconds` that is 60 by default.
> If the locators take a long time to run you may need to set a higher value.

### Example

Before:

```C#
builder.Services.AddSingleton<IStaticResourcesInfoProvider>(
	new StaticResourcesInfoProvider(manuallyManagedListOfPages)
	.Add(builder.Environment.GetWebRootCssResources(["**/site.css", "**/*.min.css"]))
	.Add(builder.Environment.GetWebRootJsResources())
	.Add(builder.Environment.GetWebRootBinResources(["**/*.ico"]))
	);

	// ...

	app.GenerateStaticContent(
		app.Environment.WebRootPath,
		exitWhenDone: exitWhenDone,
		alwaysDefaultFile: false,
		dontUpdateLinks: false,
		dontOptimizeContent: false,
		regenerationInterval: regenInterval);
```

After (replacing manual page list with the built-in sitemap resource locator)

```C#
builder.Services.AddAspNetStaticContribStekeblad(opts => // NEW!
	opts.RegisterSitemapResourceLocator = true; // NEW!
);

builder.Services.AddSingleton<IStaticResourcesInfoProvider>(
	new LocatingStaticResourcesInfoProvider() // NEW!
	.AddSitemapLocator() // NEW!
	.Add(builder.Environment.GetWebRootCssResources(["**/site.css", "**/*.min.css"]))
	.Add(builder.Environment.GetWebRootJsResources())
	.Add(builder.Environment.GetWebRootBinResources(["**/*.ico"]))
	);

	// ...

	var signal = new EventWaitHandle(false, EventResetMode.ManualReset); // NEW!
	app.LocateStaticResources(signal); // NEW!

	app.GenerateStaticContent(
		app.Environment.WebRootPath,
		exitWhenDone: exitWhenDone,
		alwaysDefaultFile: false,
		dontUpdateLinks: false,
		dontOptimizeContent: false,
		regenerationInterval: regenInterval,
		signalTimeoutSeconds: 60, // NEW!
		processStartSignal: signal); // NEW!
```

## Built-in Resource Locators

### ActionDescriptorPageResourceLocator

This resource locator discovers compiled Razor pages by processing the ASP.NET Core collection
with all registered ActionDescriptors and creates PageResources for them. It does not return any MVC-pages.

### SitemapPageResourceLocator

This resource locator discovers pages on the website by retrieving the sitemap and creates PageResources for each item.
By default it looks at the path /sitemap.xml but a custom list of paths can be configured.

## Get More Control Over Built-in Resource Locators

When you manually define resources to be included there are a number of properties that
can be set on the resource objects. To not lose this ability when using resource locators
their required constructor argument have properties to optionally define a method to be called
for each item where you can take full control over creating the resource object.

## Creating a Custom Resource Locator

Custom resource locators must derive from `ResourceLocatorBase`. This base class
force two things on implementations:

- A constructor that forwards an instance of `ResourceLocatorOptions`. Derive from
this options type if you need to take in configuration for your custom resource locator.

- An implementation of the abstract method
`Task<IEnumerable<ResourceInfoBase>> LocateResourcesAsync(IServiceProvider, ResourceLocatorFilter)`.
This is here the job of locating resources is performed.

> [!Important]
> Resource Locators is not created by DI and does not support dependency injection for their constructor,
instead they receive an instance of `IServiceProvider` when it's time to locate resources.
Use that instance to retrieve all required services, assuming they have been registered during startup.

> [!Important]
> Do not manually call `LocateResourcesAsync` on a resource locator, it will be
called by the `LocatingStaticResourcesInfoProvider` when the application has fully started.

There are four things you need to to in `LocateResourcesAsync`:

1. Locate the resources the static site generator needs to know about.
1. Determine the path to each resource
1. Create objects of the correct ResourceInfoBase-deriving type for each resource.
1. Return a list with all discovered resources.

> [!Note]
> If you are creating a reusable resource locator, use the `*ResourceFactory` property
> instead of creating resources directly in the locator. Custom resource factories can be provided
> via `ResourceLocatorOptions` in the constructor.

See this example:

```C#
public override async Task<IEnumerable<ResourceInfoBase>> LocateResourcesAsync(IServiceProvider serviceProvider,
	ResourceLocatorFilter locatorFilter)
{
	// 0. Retrieve needed services
	var contentService = serviceProvider.GetRequiredService<IMyContentService>();
	
	List<PageResource> resources = [];
	bool skipPages = locatorFilter.SkipPageResources;
	bool skipImages = locatorFilter.SkipBinResources;

	// 1. Locate the resources the static site generator needs to know about
	var allContent = await contentService.GetAllContentAsync();
	foreach (var content in allContent)
	{
		// 2. Determine the path to each resource
		string path = content.Path;

		//3. Create objects of the correct ResourceInfoBase-deriving type for each resource
		if (content.IsPage && !skipPages)
		{
			var pageResource = PageResourceFactory(path); // Avoid new PageResource(path)
			resources.Add(pageResource);
		}
		else if (content.IsImage && !skipImages)
		{
			var binResource = BinResourceFactory(path); // Avoid new BinResource(path)
			resources.Add(binResource);
		}
	}

	// 4. Return a list with all discovered resources
	return resources;
}
```
