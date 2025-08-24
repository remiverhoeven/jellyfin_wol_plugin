using System;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Jellyfin.Plugin.WolWaker.Services;
using Jellyfin.Plugin.WolWaker.Configuration;
using Jellyfin.Plugin.WolWaker.Controllers;
using Jellyfin.Plugin.WolWaker;
using Jellyfin.Data.Events;
using Jellyfin.Data.Events.System;
using Jellyfin.Data.Events.Users;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Playback;
using MediaBrowser.Model.Entities;

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
    public void RegisterServices(IServiceCollection serviceCollection)
    {
        // Register core services
        serviceCollection.AddSingleton<WolService>();
        serviceCollection.AddSingleton<PlaybackMonitorService>();
        serviceCollection.AddScoped<WolController>();
        
        // Register the plugin instance
        serviceCollection.AddSingleton<Plugin>();
        
        // Register configuration
        serviceCollection.AddSingleton<PluginConfiguration>();
    }

    /// <summary>
    /// Subscribes to Jellyfin events after the application has started.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public void SubscribeToEvents(IServiceProvider serviceProvider)
    {
        try
        {
            var logger = serviceProvider.GetRequiredService<ILogger<PluginServiceRegistrator>>();
            var playbackMonitor = serviceProvider.GetRequiredService<PlaybackMonitorService>();
            var eventManager = serviceProvider.GetRequiredService<IEventManager>();
            var libraryManager = serviceProvider.GetRequiredService<ILibraryManager>();

            // Subscribe to playback start events
            eventManager.Subscribe<PlaybackStartEventArgs>(playbackMonitor.HandlePlaybackStartAsync);
            
            // Subscribe to library item access events
            eventManager.Subscribe<ItemAccessEventArgs>(playbackMonitor.HandleItemAccessAsync);

            logger.LogInformation("WoL Waker plugin events subscribed successfully");
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<PluginServiceRegistrator>>();
            logger.LogError(ex, "Failed to subscribe to Jellyfin events");
        }
    }
}
