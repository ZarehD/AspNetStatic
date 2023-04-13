using AspNetStatic;

namespace PartialStaticSite
{
	public static class SampleStaticPages
	{
		public static IEnumerable<PageInfo> GetCollection() =>
			new PageInfo[]
			{
				new("/"),
				new("/") { QueryString = "?p1=v1", OutFilePathname = "index-p1v1.htm" },
				new("/123") { OutFilePathname = "index-123.htm" },
				new("/123") { QueryString = "?p1=v1", OutFilePathname = "index-123-p1v1.htm" },
				new("/Privacy"),
				new("/Blog/Articles/"),
				new("/Blog/Articles/Article1"),
				new("/Blog/Posts/1") { OutFilePathname = @"blog\post1.htm" },
			};
	}
}
