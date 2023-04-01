using AspNetStatic;
using StaticSiteLib;

namespace Sample.PartialStaticSite
{
	public class SampleStaticPagesInfoProvider : StaticPagesInfoProviderBase
	{
		public SampleStaticPagesInfoProvider()
		{
			this.pages.AddRange(
				new PageInfo[]
				{
					new("/"),
					new("/Privacy"),
					new("/Blog/Articles/"),
					new("/Blog/Articles/Article1"),
				});
		}
	}
}
