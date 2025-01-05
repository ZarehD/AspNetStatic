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

/// <summary>
///		Implements an object that represents the result of an optimizer operation.
/// </summary>
/// <typeparam name="TContent">The type of the optimized content.</typeparam>
public class OptimizerResult<TContent> where TContent : notnull
{
	public TContent OptimizedContent { get; set; }
	public IList<OptimizerErrorInfo> Errors { get; set; } = Array.Empty<OptimizerErrorInfo>();
	public IList<OptimizerErrorInfo> Warnings { get; set; } = Array.Empty<OptimizerErrorInfo>();

	public bool HasErrors => this.Errors.Count > 0;
	public bool HasWarnings => this.Warnings.Count > 0;


	public OptimizerResult(TContent optimizedContent)
	{
		this.OptimizedContent = optimizedContent;
	}
	public OptimizerResult(TContent optimizedContent, OptimizerErrorInfo[] errors) :
		this(optimizedContent)
	{
		this.Errors = errors;
	}
	public OptimizerResult(TContent optimizedContent, OptimizerErrorInfo[] errors, OptimizerErrorInfo[] warnings) :
		this(optimizedContent)
	{
		this.Errors = errors;
		this.Warnings = warnings;
	}
	public OptimizerResult(OptimizerErrorInfo[] errors)
	{
		this.OptimizedContent = default!;
		this.Errors = errors;
	}
}
