namespace AspNetStatic
{
	public interface IStaticPagesInfoProvider
	{
		IEnumerable<PageInfo> Pages { get; }

		/// <summary>
		///		<para>
		///			Gets the name (without an extension) to use for the default file 
		///			created routes that end wuth a slash.
		///		</para>
		///		<para>
		///			Example: "index"
		///		</para>
		/// </summary>
		string DefaultFileName { get; }

		/// <summary>
		///		<para>
		///			Gets the file extension to use for generated static page files.
		///		</para>
		///		<para>
		///			Example: ".html"
		///		</para>
		/// </summary>
		string PageFileExtension { get; }

		/// <summary>
		///		<para>
		///			Gets a collection of page/file names (without file extension) 
		///			for which a default file will not be creaed (an html file will 
		///			be created instead).
		///		</para>
		///		<para>
		///			If "index" is in the list, a route for a page /index will create 
		///			the file /index.html instead of /index/index.html. However, for 
		///			a route /index/ the result will be /index/index.html.
		///		</para>
		/// </summary>
		string[] DefaultFileExclusions { get; }
	}
}
