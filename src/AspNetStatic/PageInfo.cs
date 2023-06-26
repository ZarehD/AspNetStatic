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
	[Serializable]
	public class PageInfo
	{
		/// <summary>
		///		Gets or sets the route to the page (e.g. /blog/article1), 
		///		including any relevant route parameter values.
		/// </summary>
		/// <remarks>
		///		The specified route must be relative to the base URI 
		///		of the site (e.g. www.mysite.com), and must not include 
		///		a query string.
		/// </remarks>
		public string Route { get; init; }

		/// <summary>
		///		Gets or sets the query string for the route specified in <see cref="Route"/>.
		/// </summary>
		/// <remarks>
		///		The specified value is appended to the value specified in 
		///		<see cref="Route"/> and should start with a '?' character.
		///		(see <see cref="Url"/> property).
		/// </remarks>
		public string? Query { get; init; }

		/// <summary>
		///		Gets the combined <see cref="Route"/> and <see cref="Query"/> value.
		/// </summary>
		public string Url => $"{this.Route}{this.Query.EnsureStartsWith('?', true)}";

		/// <summary>
		///		Gets or sets the pathname (path and filename) of the file 
		///		that will be created for this page.
		/// </summary>
		/// <remarks>
		///		<para>
		///			The specified pathname must be relative to the root folder 
		///			where generated static files will be placed. Directory 
		///			separator characters at the start of the value will be 
		///			automatically removed, but other absolute path specifiers 
		///			will not.
		///		</para>
		///		<para>
		///			The value specified for this property will override any logic 
		///			used for constructing the pathname of the generated file for 
		///			this page, including any file extension for the file.
		///		</para>
		/// </remarks>
		public string? OutFile { get; init; }

		public bool SkipOptimization => this.OptimizerType == OptimizerType.None;

		public OptimizerType OptimizerType { get; init; } = OptimizerType.Auto;

		public EncodingType OutputEncoding { get; init; } = EncodingType.UTF8;

		public ChangeFrequency ChangeFrequency { get; init; } = ChangeFrequency.Never;

		public DateTime LastModified { get; init; } = DateTime.MinValue;

		public double IndexPriority { get; init; }


		public PageInfo(string route)
		{
			Throw.IfNullOrWhiteSpace(
				route, nameof(route),
				Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace);

			this.Route = route;
		}
	}


	public enum ChangeFrequency { Always, Hourly, Daily, Weekly, Monthly, Yearly, Never }

	public enum OptimizerType
	{
		Auto, // Auto-select based on OutFile filename extension. Defaults to Html.
		None, // no optimization
		Html, Xhtml, Xml
	}
}
