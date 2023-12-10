using AspNetStatic;

namespace Sample.BlazorSsg
{
	public static class StaticResourcesInfo
	{
		public static StaticResourcesInfoProvider GetProvider()
		{
			var cssResources = GetCssResources();
			var binResources = GetBinaryResources();
			var otherResources = new List<NonPageResource>();

			if (cssResources is not null) otherResources.AddRange(cssResources);
			if (binResources is not null) otherResources.AddRange(binResources);

			return new StaticResourcesInfoProvider(GetPageResources(), otherResources);
		}

		public static IEnumerable<PageResource> GetPageResources() =>
			new PageResource[]
			{
				new("/") { OutFile = "Index.html" },
				new("/Weather"),
			};

		public static IEnumerable<CssResource> GetCssResources() =>
			new CssResource[]
			{
				new("/bootstrap/bootstrap.min.css"),
				new("/app.css"),
				new("/Sample.BlazorSSG.styles.css")
			};

		public static IEnumerable<BinResource> GetBinaryResources() =>
			new BinResource[]
			{
				new("/favicon.png"),
			};

	}
}
