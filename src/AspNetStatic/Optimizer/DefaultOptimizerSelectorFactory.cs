/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023-2025 Zareh DerGevorkian

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License. 
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
--------------------------------------------------------------------------------------------------------------------------------*/

using Microsoft.Extensions.DependencyInjection;
using WebMarkupMin.Core;

namespace AspNetStatic.Optimizer;

public static class DefaultOptimizerSelectorFactory
{
	public static IOptimizerSelector Create(IServiceProvider services)
	{
		var markupOptimizer = services.GetService<IMarkupOptimizer>();
		var cssOptimizer = services.GetService<ICssOptimizer>();
		var jsOptimizer = services.GetService<IJsOptimizer>();
		var binOptimizer = services.GetService<IBinOptimizer>() ?? new NullBinOptimizer();

		var cssMinifier = services.GetService<ICssMinifier>();
		var jsMinifier = services.GetService<IJsMinifier>();

		if ((markupOptimizer is null) || (cssOptimizer is null) || (jsOptimizer is null))
		{
			cssMinifier ??= new KristensenCssMinifier();
			jsMinifier ??= new CrockfordJsMinifier();
		}

		markupOptimizer ??= new DefaultMarkupOptimizer(
			services.GetService<HtmlMinificationSettings>(),
			services.GetService<XhtmlMinificationSettings>(),
			services.GetService<XmlMinificationSettings>(),
			cssMinifier, jsMinifier);

		cssOptimizer ??= new DefaultCssOptimizer(cssMinifier!);

		jsOptimizer ??= new DefaultJsOptimizer(jsMinifier!);

		return
			new DefaultOptimizerSelector(
				markupOptimizer,
				cssOptimizer,
				jsOptimizer,
				binOptimizer);
	}
}
