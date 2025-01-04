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

public class OptimizerErrorInfo(
	string category, string message,
	int lineNumber, int columnNumber,
	string sourceFragment)
{
	public string Category { get; } = category;
	public string Message { get; } = message;
	public int LineNumber { get; } = lineNumber;
	public int ColumnNumber { get; } = columnNumber;
	public string SourceFragment { get; } = sourceFragment;


	public OptimizerErrorInfo(string message)
		: this(string.Empty, message, 0, 0, string.Empty)
	{ }
	public OptimizerErrorInfo(string category, string message)
		: this(category, message, 0, 0, string.Empty)
	{ }
	public OptimizerErrorInfo(string message, int lineNumber, int columnNumber, string sourceFragment)
		: this(string.Empty, message, lineNumber, columnNumber, sourceFragment)
	{ }

	public override string ToString() =>
		$"{(string.IsNullOrWhiteSpace(this.Category) ? null : $"{this.Category}: ")}" +
		$"{this.Message}; L:{this.LineNumber}, C:{this.ColumnNumber}; S:{this.SourceFragment}";


	public static implicit operator OptimizerErrorInfo(MinificationErrorInfo webMinErr) =>
		new(webMinErr.Category,
			webMinErr.Message,
			webMinErr.LineNumber,
			webMinErr.ColumnNumber,
			webMinErr.SourceFragment);

	public static implicit operator string(OptimizerErrorInfo err) => err.ToString();

}
