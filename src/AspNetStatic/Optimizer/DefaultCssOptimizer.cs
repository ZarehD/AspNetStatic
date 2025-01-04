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

public class DefaultCssOptimizer(
	ICssMinifier cssMinifier) :
	ICssOptimizer
{
	protected readonly ICssMinifier _cssMinifier = cssMinifier;

	public virtual CssOptimizerResult Execute(string content, CssResource resource, string outFilePathname) =>
		resource.OptimizationType == OptimizationType.None
		? new(content) :
		this._cssMinifier.Minify(
			content, false,
			resource.OutputEncoding.ToSystemEncoding());
}
