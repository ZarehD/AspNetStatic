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

public interface IJsOptimizer
{
	JsOptimizerResult Execute(string content, JsResource resource, string outFilePathname);
}


public class JsOptimizerResult : OptimizerResult<string>
{
	public JsOptimizerResult(string optimizedContent) : base(optimizedContent) { }
	public JsOptimizerResult(string optimizedContent, OptimizerErrorInfo[] errors) : base(optimizedContent, errors) { }
	public JsOptimizerResult(string optimizedContent, OptimizerErrorInfo[] errors, OptimizerErrorInfo[] warnings) : base(optimizedContent, errors, warnings) { }
	public JsOptimizerResult(OptimizerErrorInfo[] errors) : base(errors) { }

	public static implicit operator JsOptimizerResult(MinificationResultBase webMinResult) =>
		new(webMinResult.MinifiedContent,
			webMinResult.Errors.Select(x => (OptimizerErrorInfo) x).ToArray(),
			webMinResult.Warnings.Select(x => (OptimizerErrorInfo) x).ToArray());
}
