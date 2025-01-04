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

public interface IOptimizerSelector
{
	IMarkupOptimizer SelectFor(PageResource pageResource, string outFilePathname);

	ICssOptimizer SelectFor(CssResource cssResource, string outFilePathname);

	IJsOptimizer SelectFor(JsResource jsResource, string outFilePathname);

	IBinOptimizer SelectFor(BinResource binResource, string outFilePathname);
}
