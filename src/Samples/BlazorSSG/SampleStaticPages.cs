using AspNetStatic;

namespace Sample.BlazorSsg
{
	public static class SampleStaticPages
	{
		public static IEnumerable<PageInfo> GetCollection() =>
			new PageInfo[]
			{
				new("/"),
				new("/Weather"),
			};
	}
}
