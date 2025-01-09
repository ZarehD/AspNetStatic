using AspNetStatic;

namespace PartialStaticSite;

public static class SampleStaticPages
{
	public static IEnumerable<PageResource> GetCollection() =>
		[
			new("/"),
			new("/") { Query = "?p1=v1", OutFile = "index-p1v1.htm" },
			new("/123") { OutFile = "index-123.htm" },
			new("/123") { Query = "?p1=v1", OutFile = "index-123-p1v1.htm" },
			new("/Privacy"),
			new("/Blog/Articles/"),
			new("/Blog/Articles/Article1"),
			new("/Blog/Posts/1") { OutFile = @"blog\post1.htm" },
		];
}
