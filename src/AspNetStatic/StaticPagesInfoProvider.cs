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
	public class StaticPagesInfoProvider : IStaticPagesInfoProvider
	{
		public IEnumerable<PageInfo> Pages => this.pages.ToArray();
		protected readonly List<PageInfo> pages = new();

		/// <inheritdoc/>
		/// <remarks>
		///		Defaults to "index".
		/// </remarks>
		public string DefaultFileName => this.defaultFileName;
		protected string defaultFileName = "index";

		/// <inheritdoc/>
		/// <remarks>
		///		Defaults to ".html".
		/// </remarks>
		public string PageFileExtension => this.defaultFileExtension;
		protected string defaultFileExtension = ".html";

		/// <inheritdoc/>
		/// <remarks>
		///		Defaults to ["index", "default"].
		/// </remarks>
		public string[] DefaultFileExclusions => this.exclusions.ToArray();
		protected readonly List<string> exclusions = new(new[] { "index", "default" });


		public StaticPagesInfoProvider(
			IEnumerable<PageInfo> pages,
			string? defaultFileName = default,
			string? defaultFileExtension = default,
			IEnumerable<string>? dffExclusions = default)
		{
			this.pages.AddRange(pages ?? throw new ArgumentNullException(nameof(pages)));

			if (defaultFileName is not null) SetDefaultFileName(defaultFileName);
			if (defaultFileExtension is not null) SetDefaultFileExtension(defaultFileExtension);
			if (dffExclusions is not null) SetDefaultFileExclusions(dffExclusions.ToArray());
		}


		protected virtual void SetDefaultFileName(string name)
		{
			this.defaultFileName =
				string.IsNullOrWhiteSpace(name)
				? throw new ArgumentException(Properties.Resources.Err_MissingDefaultFileName, nameof(name))
				: name;
		}
		protected virtual void SetDefaultFileExtension(string extension)
		{
			this.defaultFileExtension =
				string.IsNullOrWhiteSpace(extension)
				? throw new ArgumentException(Properties.Resources.Err_MissingDefaultFileExtension, nameof(extension))
				: extension;
		}
		protected virtual void SetDefaultFileExclusions(string[] newExclusions)
		{
			if (newExclusions.Any(s => string.IsNullOrWhiteSpace(s)))
			{
				throw new ArgumentException(
					Properties.Resources.Err_ArrayElementNullOrWhitespace,
					nameof(newExclusions));
			}

			this.exclusions.Clear();
			this.exclusions.AddRange(newExclusions);
		}
	}
}
