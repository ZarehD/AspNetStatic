using AspNetStatic;

namespace AspNetStaticContrib.Stekeblad.Options
{
	/// <summary>
	/// <para>
	/// Base options type for Resource Locators. Allows defining
	/// methods for how various resource types are created.
	/// </para>
	/// <para>
	/// Custom resource locator may use this instance
	/// or derive from it to add additional options.
	/// </para>
	/// </summary>
	public class ResourceLocatorOptions
	{
		/// <summary>
		/// Optionally define a custom method for creating <see cref="PageResource"/> objects
		/// if the resource locator does not create them like you want.
		/// </summary>
		public Func<string, PageResource>? PageResourceFactory { get; set; }

		/// <summary>
		/// Optionally define a custom method for creating <see cref="BinResource"/> objects
		/// if the resource locator does not create them like you want.
		/// </summary>
		public Func<string, BinResource>? BinResourceFactory { get; set; }

		/// <summary>
		/// Optionally define a custom method for creating <see cref="JsResource"/> objects
		/// if the resource locator does not create them like you want.
		/// </summary>
		public Func<string, JsResource>? JsResourceFactory { get; set; }

		/// <summary>
		/// Optionally define a custom method for creating <see cref="CssResource"/> objects
		/// if the resource locator does not create them like you want.
		/// </summary>
		public Func<string, CssResource>? CssResourceFactory { get; set; }
	}
}
