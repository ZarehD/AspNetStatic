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

namespace AspNetStatic.Optimizer;

public class DefaultMarkupOptimizer(
	HtmlMinificationSettings? htmlMinifierSettings = default,
	XhtmlMinificationSettings? xhtmlMinifierSettings = default,
	XmlMinificationSettings? xmlMinifierSettings = default,
	ICssMinifier? cssMinifier = default,
	IJsMinifier? jsMinifier = default) : 
	IMarkupOptimizer
{
	protected readonly IMarkupMinifier _htmlMinifier = new HtmlMinifier(htmlMinifierSettings, cssMinifier, jsMinifier);
	protected readonly IMarkupMinifier _xhtmlMinifier = new XhtmlMinifier(xhtmlMinifierSettings, cssMinifier, jsMinifier);
	protected readonly IMarkupMinifier _xmlMinifier = new XmlMinifier(xmlMinifierSettings);


	public virtual MarkupOptimizerResult Execute(string content, PageResource resource, string outFilePathname)
	{
		if (resource.OptimizationType == OptimizationType.None) return new(content);

		var optimizer =
			resource.OptimizationType == OptimizationType.Auto
			? Path.GetExtension(outFilePathname).ToLowerInvariant() switch
			{
				".html" or ".htm" => this._htmlMinifier,
				".xhtml" or ".xhtm" => this._xhtmlMinifier,
				".xml" => this._xmlMinifier,
				_ => this._htmlMinifier
			}
			: resource.OptimizationType switch
			{
				OptimizationType.Html => this._htmlMinifier,
				OptimizationType.Xhtml => this._xhtmlMinifier,
				OptimizationType.Xml => this._xmlMinifier,
				_ => this._htmlMinifier
			};

		return optimizer.Minify(content, resource.OutputEncoding.ToSystemEncoding());
	}
}
