/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023 Zareh DerGevorkian

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License. 
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
--------------------------------------------------------------------------------------------------------------------------------*/

namespace AspNetStatic
{
	public interface IBinOptimizer
	{
		BinOptimizerResult Execute(byte[] content, BinResource resource, string outFilePathname);
	}


	public class BinOptimizerResult
	{
		public byte[] OptimizedContent { get; }
		public IEnumerable<string> Errors { get; }
		public IEnumerable<string> Warnings { get; }


		public BinOptimizerResult(byte[] optimizedContent)
			: this(optimizedContent,
				  Enumerable.Empty<string>(),
				  Enumerable.Empty<string>())
		{ }

		public BinOptimizerResult(byte[] optimizedContent, IEnumerable<string> errors)
			: this(optimizedContent, errors, Enumerable.Empty<string>())
		{ }

		public BinOptimizerResult(byte[] optimizedContent, IEnumerable<string> errors, IEnumerable<string> warnings)
		{
			this.OptimizedContent = optimizedContent;
			this.Errors = errors;
			this.Warnings = warnings;
		}
	}
}
