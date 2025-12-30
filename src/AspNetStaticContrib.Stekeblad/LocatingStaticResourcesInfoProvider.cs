using AspNetStatic;
using AspNetStaticContrib.Stekeblad.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ThrowGuard;

namespace AspNetStaticContrib.Stekeblad
{
	/// <summary>
	/// An extension of <see cref="StaticResourcesInfoProvider"/> supporting the concept of
	/// <see cref="ResourceLocatorBase">Resource Locators</see> that can discover resources after the
	/// application has started by utilizing all registered services.
	/// </summary>
	public class LocatingStaticResourcesInfoProvider : StaticResourcesInfoProvider
	{
		private readonly List<ResourceLocatorBase> _resourceLocators;
		private bool hasLocatedResources;

		public LocatingStaticResourcesInfoProvider(IEnumerable<ResourceInfoBase>? resources = default,
			string? defaultFileName = default,
			string? defaultFileExtension = default,
			IEnumerable<string>? defaultFileExclusions = default)
			: base(resources, defaultFileName, defaultFileExtension, defaultFileExclusions)
		{
			_resourceLocators = [];
		}

		/// <summary>
		/// Adds a Resource Locator to be called before static generation starts.
		/// If you directly or indirectly call this method then you need to call
		/// <see cref="Extensions.IHostExtensions.LocateStaticResources" /> before
		/// <see cref="StaticGeneratorHostExtension.GenerateStaticContent"/>
		/// </summary>
		/// <param name="resourceLocator">The resource locator to add</param>
		/// <returns>The current instance, to enable chaining of add calls</returns>
		/// <exception cref="InvalidOperationException">If resources already have been located.
		/// Resources can not be added or removed after generation has started or between updates
		/// (if update interval is configured)
		/// </exception>
		public LocatingStaticResourcesInfoProvider AddResourceLocator(ResourceLocatorBase resourceLocator)
		{
			if (hasLocatedResources)
				Throw.InvalidOp("Can't add new resource locator, resources has already been located");

			_resourceLocators.Add(resourceLocator);
			return this;
		}

		/// <summary>
		/// <para>
		/// Called <see cref="Extensions.IHostExtensions.LocateStaticResources">IHostExtensions.LocateStaticResources</see>
		/// </para>
		/// <para>
		/// Calls all registered resource locators one by one and sets
		/// the <paramref name="signal"/> when all of them have finished
		/// </para>
		/// </summary>
		/// <param name="services">A service provider to forward to all registered resource locators</param>
		/// <param name="signal">A signal that can be awaited until all resources has been located</param>
		internal async Task LocateResourcesAsync(IServiceProvider services, EventWaitHandle signal)
		{
			var loggerFactory = services.GetRequiredService<ILoggerFactory>();
			var logger = loggerFactory.CreateLogger<LocatingStaticResourcesInfoProvider>();

			if (hasLocatedResources)
			{
				logger.LogInformation("Skipping LocateResourcesAsync, resources has already been located");
				signal.Set();
				return;
			}

			hasLocatedResources = true;

			// This method gets called on IHostApplicationLifetime.ApplicationStarted. To not
			// block others registered on that event we yield to avoid a resource locator
			// running a long time synchronously without awaiting and ending up blocking other code.
			await Task.Yield();

			logger.LogInformation("Calling resource locators... ({numLocators} added)", _resourceLocators.Count);
			var locatorFilter = CreateLocatorFilter();
			foreach (var resourceLocator in _resourceLocators)
			{
				try
				{
					if (logger.IsEnabled(LogLevel.Information))
						logger.LogInformation("Calling resource locator {resourceLocator}", resourceLocator.GetType().Name);

					var resources = await resourceLocator.LocateResourcesAsync(services, locatorFilter);
					base.resources.AddRange(resources);
				}
				catch (Exception ex)
				{
					if (logger.IsEnabled(LogLevel.Error))
						logger.LogError(ex, "Resource locator {resourceLocator} threw an exception", resourceLocator.GetType().Name);
				}
			}

			logger.LogInformation("All resource locators called, setting signal");
			signal.Set();
		}

		protected ResourceLocatorFilter CreateLocatorFilter() =>
			new()
			{
				SkipBinResources = SkipBinResources,
				SkipCssResources = SkipCssResources,
				SkipJsResources = SkipJsResources,
				SkipPageResources = SkipPageResources
			};
	}
}
