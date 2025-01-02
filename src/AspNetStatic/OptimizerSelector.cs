/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023-2025 Zareh DerGevorkian

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
		protected readonly IMarkupMinifier _nullMarkupMinifier = new NullMarkupMinifier();
		protected readonly ICssMinifier _nullCssMinifier = new NullCssMinifier();
		protected readonly IJsMinifier _nullJsMinifier = new NullJsMinifier();
		protected readonly IMarkupMinifier _htmlMinifier;
		protected readonly IMarkupMinifier _xhtmlMinifier;
		protected readonly IMarkupMinifier _xmlMinifier;
		protected readonly ICssMinifier _cssMinifier;
		protected readonly IJsMinifier _jsMinifier;
		protected readonly IBinOptimizer? _binOptimizer;


		public OptimizerSelector(
			IMarkupMinifier htmlMinifier,
			IMarkupMinifier xhtmlMinifier,
			IMarkupMinifier xmlMinifier,
			ICssMinifier cssMinifier,
			IJsMinifier jsMinifier,
			IBinOptimizer? binOptimizer)
		{
			this._htmlMinifier = Throw.IfNull(htmlMinifier);
			this._xhtmlMinifier = Throw.IfNull(xhtmlMinifier);
			this._xmlMinifier = Throw.IfNull(xmlMinifier);
			this._cssMinifier = Throw.IfNull(cssMinifier);
			this._jsMinifier = Throw.IfNull(jsMinifier);
			this._binOptimizer = binOptimizer;
		}


		public virtual IMarkupMinifier SelectFor(PageResource page, string outFilePathname) =>
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

		public virtual ICssMinifier SelectFor(CssResource cssResource, string outFilePathname) =>
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

		public virtual IJsMinifier SelectFor(JsResource jsResource, string outFilePathname) =>
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

		public virtual IBinOptimizer? SelectFor(BinResource binResource, string outFilePathname) =>
			binResource.OptimizerType == OptimizerType.Bin ? this._binOptimizer : null;
	}
}
