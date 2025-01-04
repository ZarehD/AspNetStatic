using AspNetStatic;

namespace Sample.BlazorSsg;

public static class StaticResourcesInfo
{
	public static StaticResourcesInfoProvider GetProvider()
	{
		var provider = new StaticResourcesInfoProvider();

		provider.Add(GetPageResources());
		provider.Add(GetCssResources());
		provider.Add(GetBinaryResources());

		return provider;
	}

	public static IEnumerable<PageResource> GetPageResources() =>
		[
			new("/") { OutFile = "Index.html" },
			new("/Weather"),
		];

	public static IEnumerable<CssResource> GetCssResources() =>
		[
			new("/bootstrap/bootstrap.min.css"),
			new("/app.css"),
			new("/Sample.BlazorSSG.styles.css")
		];

	public static IEnumerable<BinResource> GetBinaryResources() =>
		[
			new("/favicon.png"),
		];

}
