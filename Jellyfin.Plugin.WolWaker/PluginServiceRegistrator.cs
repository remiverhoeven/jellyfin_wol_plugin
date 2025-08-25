using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Jellyfin.Plugin.WolWaker.Services;
using Jellyfin.Plugin.WolWaker;
using Jellyfin.Plugin.WolWaker.Controllers;

namespace Jellyfin.Plugin.WolWaker;

/// <summary>
/// Registers plugin services with Jellyfin's dependency injection container.
/// </summary>
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <summary>
    /// Registers plugin services with the service collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection to register services with.</param>
    /// <param name="applicationHost">The application host.</param>
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        // Register core services
        serviceCollection.AddSingleton<WolService>();
        serviceCollection.AddSingleton<PlaybackMonitorService>();
        serviceCollection.AddScoped<WolController>();
        
        // Register the plugin instance
        serviceCollection.AddSingleton<Plugin>();
        
        // Register configuration
        serviceCollection.AddSingleton<PluginConfiguration>();
        
        // Register event subscription service
        serviceCollection.AddHostedService<EventSubscriptionService>();
    }
}
