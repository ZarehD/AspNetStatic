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



		protected virtual void SetDefaultFileName(string name) => this.defaultFileName = Throw.IfNullOrWhiteSpace(name, nameof(name), Properties.Resources.Err_MissingDefaultFileName);
		protected virtual void SetDefaultFileExtension(string extension) => this.defaultFileExtension = Throw.IfNullOrWhiteSpace(extension, nameof(extension), Properties.Resources.Err_MissingDefaultFileExtension);
		protected virtual void SetDefaultFileExclusions(string[] newExclusions)
		{
			if (newExclusions.Any(s => string.IsNullOrWhiteSpace(s)))
			{
				Throw.BadArg(
					nameof(newExclusions),
					Properties.Resources.Err_ArrayElementNullOrWhitespace);
			}

			this.exclusions.Clear();
			this.exclusions.AddRange(newExclusions);
		}
	}
}
