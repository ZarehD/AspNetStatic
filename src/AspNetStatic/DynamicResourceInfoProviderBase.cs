namespace AspNetStatic
{
	/// <summary>
	/// When resources to statically generate is not known at startup and can't be
	/// determined during early startup, then a type deriving from
	/// <see cref="DynamicResourceInfoProviderBase" />
	/// could be the solution as it runs after the application is fully build,
	/// giving you access to all registered services.
	/// </summary>
	public abstract class DynamicResourceInfoProviderBase : StaticResourcesInfoProviderBase
	{
		public abstract Task DiscoverResourcesAsync(IServiceProvider serviceProvider);
	}
}
