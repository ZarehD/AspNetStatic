/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023-2025 Zareh DerGevorkian

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License. 
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
--------------------------------------------------------------------------------------------------------------------------------*/

namespace AspNetStatic.Optimizer;

public class DefaultOptimizerSelector : IOptimizerSelector
{
	protected readonly IMarkupOptimizer _nullMarkupOptimizer = new NullMarkupOptimizer();
	protected readonly ICssOptimizer _nullCssOptimizer = new NullCssOptimizer();
	protected readonly IJsOptimizer _nullJsOptimizer = new NullJsOptimizer();
	protected readonly IBinOptimizer _nullBinOptimizer = new NullBinOptimizer();

	protected readonly IMarkupOptimizer _markupOptimizer;
	protected readonly ICssOptimizer _cssOptimizer;
	protected readonly IJsOptimizer _jsOptimizer;
	protected readonly IBinOptimizer _binOptimizer;

	public DefaultOptimizerSelector(
		IMarkupOptimizer? markupOptimizer,
		ICssOptimizer? cssOptimizer,
		IJsOptimizer? jsOptimizer,
		IBinOptimizer? binOptimizer)
	{
		this._markupOptimizer = markupOptimizer ?? this._nullMarkupOptimizer;
		this._cssOptimizer = cssOptimizer ?? this._nullCssOptimizer;
		this._jsOptimizer = jsOptimizer ?? this._nullJsOptimizer;
		this._binOptimizer = binOptimizer ?? this._nullBinOptimizer;
	}

	public virtual IMarkupOptimizer SelectFor(PageResource pageResource, string outFilePathname) =>
		pageResource.OptimizationType == OptimizationType.None
		? this._nullMarkupOptimizer
		: this._markupOptimizer;
	public virtual ICssOptimizer SelectFor(CssResource cssResource, string outFilePathname) =>
		cssResource.OptimizationType == OptimizationType.None
		? this._nullCssOptimizer
		: this._cssOptimizer;
	public virtual IJsOptimizer SelectFor(JsResource jsResource, string outFilePathname) =>
		jsResource.OptimizationType == OptimizationType.None
		? this._nullJsOptimizer
		: this._jsOptimizer;
	public virtual IBinOptimizer SelectFor(BinResource binResource, string outFilePathname) =>
		binResource.OptimizationType == OptimizationType.None
		? this._nullBinOptimizer
		: this._binOptimizer;


	#region OBSOLETE
	//protected readonly IMarkupMinifier _nullMarkupMinifier = new NullMarkupMinifier();
	//protected readonly ICssMinifier _nullCssMinifier = new NullCssMinifier();
	//protected readonly IJsMinifier _nullJsMinifier = new NullJsMinifier();

	//protected readonly IMarkupMinifier _htmlMinifier;
	//protected readonly IMarkupMinifier _xhtmlMinifier;
	//protected readonly IMarkupMinifier _xmlMinifier;
	//protected readonly ICssMinifier _cssMinifier;
	//protected readonly IJsMinifier _jsMinifier;

	//public virtual IMarkupMinifier X_SelectFor(PageResource page, string outFilePathname) =>
	//	page.OptimizerType == OptimizerType.None ? this._nullMarkupMinifier :
	//	page.OptimizerType == OptimizerType.Auto
	//	? Path.GetExtension(outFilePathname).ToLowerInvariant() switch
	//	{
	//		".html" or ".htm" => this._htmlMinifier,
	//		".xhtml" or ".xhtm" => this._xhtmlMinifier,
	//		".xml" => this._xmlMinifier,
	//		_ => this._htmlMinifier
	//	}
	//	: page.OptimizerType switch
	//	{
	//		OptimizerType.Html => this._htmlMinifier,
	//		OptimizerType.Xhtml => this._xhtmlMinifier,
	//		OptimizerType.Xml => this._xmlMinifier,
	//		_ => this._htmlMinifier
	//	};

	//public virtual ICssMinifier X_SelectFor(CssResource cssResource, string outFilePathname) =>
	//	cssResource.OptimizerType == OptimizerType.None ? this._nullCssMinifier :
	//	cssResource.OptimizerType == OptimizerType.Auto
	//	? Path.GetExtension(outFilePathname).ToLowerInvariant() switch
	//	{
	//		".css" => this._cssMinifier,
	//		_ => this._nullCssMinifier
	//	}
	//	: cssResource.OptimizerType switch
	//	{
	//		OptimizerType.Css => this._cssMinifier,
	//		_ => this._nullCssMinifier
	//	};

	//public virtual IJsMinifier X_SelectFor(JsResource jsResource, string outFilePathname) =>
	//	jsResource.OptimizerType == OptimizerType.None ? this._nullJsMinifier :
	//	jsResource.OptimizerType == OptimizerType.Auto
	//	? Path.GetExtension(outFilePathname).ToLowerInvariant() switch
	//	{
	//		".js" or ".json" => this._jsMinifier,
	//		_ => this._nullJsMinifier
	//	}
	//	: jsResource.OptimizerType switch
	//	{
	//		OptimizerType.Js => this._jsMinifier,
	//		_ => this._nullJsMinifier
	//	}; 
	#endregion
}
