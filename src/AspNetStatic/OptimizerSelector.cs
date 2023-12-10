/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023 Zareh DerGevorkian

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License. 
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
--------------------------------------------------------------------------------------------------------------------------------*/

using WebMarkupMin.Core;

namespace AspNetStatic
{
	public class OptimizerSelector : IOptimizerSelector
	{
		private readonly IMarkupMinifier _nullMarkupMinifier = new NullMarkupMinifier();
		private readonly ICssMinifier _nullCssMinifier = new NullCssMinifier();
		private readonly IJsMinifier _nullJsMinifier = new NullJsMinifier();
		private readonly IMarkupMinifier _htmlMinifier;
		private readonly IMarkupMinifier _xhtmlMinifier;
		private readonly IMarkupMinifier _xmlMinifier;
		private readonly ICssMinifier _cssMinifier;
		private readonly IJsMinifier _jsMinifier;


		public OptimizerSelector(
			IMarkupMinifier htmlMinifier,
			IMarkupMinifier xhtmlMinifier,
			IMarkupMinifier xmlMinifier,
			ICssMinifier cssMinifier,
			IJsMinifier jsMinifier)
		{
			this._htmlMinifier = Throw.IfNull(htmlMinifier);
			this._xhtmlMinifier = Throw.IfNull(xhtmlMinifier);
			this._xmlMinifier = Throw.IfNull(xmlMinifier);
			this._cssMinifier = Throw.IfNull(cssMinifier);
			this._jsMinifier = Throw.IfNull(jsMinifier);
		}


		public IMarkupMinifier SelectFor(PageResource page, string outFilePathname) =>
			page.OptimizerType == OptimizerType.None ? this._nullMarkupMinifier :
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

		public ICssMinifier SelectFor(CssResource cssResource, string outFilePathname) =>
			cssResource.OptimizerType == OptimizerType.None ? this._nullCssMinifier :
			cssResource.OptimizerType == OptimizerType.Auto
			? Path.GetExtension(outFilePathname).ToLowerInvariant() switch
			{
				".css" => this._cssMinifier,
				_ => this._nullCssMinifier
			}
			: cssResource.OptimizerType switch
			{
				OptimizerType.Css => this._cssMinifier,
				_ => this._nullCssMinifier
			};

		public IJsMinifier SelectFor(JsResource jsResource, string outFilePathname) =>
			jsResource.OptimizerType == OptimizerType.None ? this._nullJsMinifier :
			jsResource.OptimizerType == OptimizerType.Auto
			? Path.GetExtension(outFilePathname).ToLowerInvariant() switch
			{
				".js" or ".json" => this._jsMinifier,
				_ => this._nullJsMinifier
			}
			: jsResource.OptimizerType switch
			{
				OptimizerType.Js => this._jsMinifier,
				_ => this._nullJsMinifier
			};

	}
}
