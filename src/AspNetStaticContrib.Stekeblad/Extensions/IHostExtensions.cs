using AspNetStatic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AspNetStaticContrib.Stekeblad.Extensions
{
	public static class IHostExtensions
	{
		/// <summary>
		/// <para>
		/// If you have added at least one <see cref="ResourceLocatorBase">ResourceLocator</see>
		/// to <see cref="LocatingStaticResourcesInfoProvider" /> then you must call this method
		/// before calling <see cref="StaticGeneratorHostExtension.GenerateStaticContent" />
		/// for the locators to be called.
		/// </para>
		/// <para>
		/// Provide the same instance of <see cref="EventWaitHandle"/> to this method
		/// and <see cref="StaticGeneratorHostExtension.GenerateStaticContent" />
		/// so the static generation don't start until all resources has been found.
		/// </para>
		/// </summary>
		/// <param name="host"></param>
		/// <param name="signal">A signal that will be set when all resource locators have finished.
		/// Provide the same instance to GenerateStaticContent.
		/// </param>
		/// <exception cref="Exception">If retrieving an instance of IStaticResourcesInfoProvider
		/// does not return an instance of LocatingStaticResourcesInfoProvider
		/// </exception>
		public static void LocateStaticResources(this IHost host, EventWaitHandle signal)
		{
			var resourceProvider = host.Services.GetService<IStaticResourcesInfoProvider>();
			if (resourceProvider is not LocatingStaticResourcesInfoProvider locatingProvider)
			{
				throw new Exception(
					$"{nameof(LocateStaticResources)} requires a service deriving from {nameof(LocatingStaticResourcesInfoProvider)} " +
					$"to be registered as the implementation provided when requesting a {nameof(IStaticResourcesInfoProvider)}");
			}

			var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
			lifetime.ApplicationStarted.Register(async () => await locatingProvider.LocateResourcesAsync(host.Services, signal));
		}
	}
}
