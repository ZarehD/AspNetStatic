using WebMarkupMin.Core;

namespace AspNetStatic
{
	public class OptimizerSelector : IOptimizerSelector
	{
		private readonly IMarkupMinifier _htmlMinifier;
		private readonly IMarkupMinifier _xhtmlMinifier;
		private readonly IMarkupMinifier _xmlMinifier;


		public OptimizerSelector(
			IMarkupMinifier htmlMinifier,
			IMarkupMinifier xhtmlMinifier,
			IMarkupMinifier xmlMinifier)
		{
			this._htmlMinifier = Throw.IfNull(htmlMinifier);
			this._xhtmlMinifier = Throw.IfNull(xhtmlMinifier);
			this._xmlMinifier = Throw.IfNull(xmlMinifier);
		}


		public IMarkupMinifier SelectFor(PageInfo page, string outFilePathname) =>
				page.OptimizerType == OptimizerType.Auto
				? Path.GetExtension(outFilePathname).ToLowerInvariant() switch
				{
					".html" or ".htm" => this._htmlMinifier,
					".xhtml" or ".xhtm" => this._xhtmlMinifier,
					".xml" => this._xmlMinifier,
					_ => this._htmlMinifier
				}
				: page.OptimizerType switch
				{
					OptimizerType.Html => this._htmlMinifier,
					OptimizerType.Xhtml => this._xhtmlMinifier,
					OptimizerType.Xml => this._xmlMinifier,
					_ => this._htmlMinifier
				};
	}
}
