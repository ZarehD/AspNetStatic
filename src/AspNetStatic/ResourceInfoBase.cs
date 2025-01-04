/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023-2025 Zareh DerGevorkian

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License. 
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
--------------------------------------------------------------------------------------------------------------------------------*/

using AspNetStatic.Optimizer;

namespace AspNetStatic;

[Serializable]
public abstract class ResourceInfoBase(string route)
{
	/// <summary>
	///		Gets or sets the route to the content item, a URI resource 
	///		(e.g. /blog/article1 or /css/site.css), including route parameter values.
	/// </summary>
	/// <remarks>
	///		The specified route must be relative to the base URI 
	///		of the site (e.g. www.mysite.com), and must not include 
	///		a query string.
	/// </remarks>
	public string Route { get; init; } = Throw.IfNullOrWhitespace(route);

	/// <summary>
	///		Gets or sets the query string for the route specified in <see cref="Route"/>.
	/// </summary>
	/// <remarks>
	///		The specified value is appended to the value specified in 
	///		<see cref="Route"/> and can start with a '?' character.
	///		(see <see cref="Url"/> property).
	/// </remarks>
	public string? Query { get; init; }

	/// <summary>
	///		Gets the combined <see cref="Route"/> and <see cref="Query"/> value.
	/// </summary>
	public string Url => $"{this.Route}{this.Query.EnsureStartsWith('?', true)}";

	/// <summary>
	///		Gets or sets the pathname (path and filename) of the file 
	///		that will be created for this content/resource.
	/// </summary>
	/// <remarks>
	///		<para>
	///			The specified pathname must be relative to the root folder 
	///			where generated static files will be placed. Directory 
	///			separator characters at the start of the value will be 
	///			automatically removed, but other absolute path specifiers 
	///			will not, and will therefore cause in error.
	///		</para>
	///		<para>
	///			The value specified for this property will override any logic 
	///			used for constructing the pathname of the generated file.
	///		</para>
	/// </remarks>
	public string? OutFile { get; init; }

	/// <summary>
	///		Gets the encoding type to use when writing textual content 
	///		to the file specified in <see cref="OutFile"/>.
	/// </summary>
	public EncodingType OutputEncoding { get; init; } = EncodingType.UTF8;

	public bool SkipOptimization => this.OptimizationType == OptimizationType.None;

	public OptimizationType OptimizationType { get; init; } = OptimizationType.Auto;
}
