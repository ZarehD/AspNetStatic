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

public static class RegisterDefaultOptimizers
{
	public static IServiceCollection AddDefaultOptimizerSelector(
		this IServiceCollection services)
	{
		Throw.IfNull(services);

		services
			.AddSingleton<IOptimizerSelector, DefaultOptimizerSelector>()
			.AddDefaultOptimizers()
			;

		return services;
	}

	public static IServiceCollection AddDefaultOptimizers(
		this IServiceCollection services)
	{
		Throw.IfNull(services);

		services
			.AddSingleton<DefaultMarkupOptimizer>()
			.AddSingleton<DefaultCssOptimizer>()
			.AddSingleton<DefaultJsOptimizer>()
			;

		services
			.AddSingleton<IMarkupOptimizer, DefaultMarkupOptimizer>()
			.AddSingleton<ICssOptimizer, DefaultCssOptimizer>()
			.AddSingleton<IJsOptimizer, DefaultJsOptimizer>()
			.AddSingleton<IBinOptimizer, NullBinOptimizer>()
			;

		services.AddDefaultMinifiers();

		return services;
	}

	public static IServiceCollection AddDefaultMinifiers(
		this IServiceCollection services)
	{
		Throw.IfNull(services);

		services
			.AddSingleton<ICssMinifier, KristensenCssMinifier>()
			.AddSingleton<IJsMinifier, CrockfordJsMinifier>()
			;

		return services;
	}
}
