namespace AspNetStatic
{
	public abstract class StaticPagesInfoProviderBase : IStaticPagesInfoProvider
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
