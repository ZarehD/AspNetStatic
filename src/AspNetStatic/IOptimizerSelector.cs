using WebMarkupMin.Core;

namespace AspNetStatic
{
	public interface IOptimizerSelector
	{
		IMarkupMinifier SelectFor(PageInfo page, string outFilePathname);
	}
}