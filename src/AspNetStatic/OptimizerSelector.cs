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
		private readonly IMarkupMinifier _nullMinifier = new NullMinifier();
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
			page.OptimizerType == OptimizerType.None ? this._nullMinifier :
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
