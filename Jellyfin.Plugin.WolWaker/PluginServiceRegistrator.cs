using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Jellyfin.Plugin.WolWaker.Services;

namespace Jellyfin.Plugin.WolWaker;

/// <summary>
/// Service registrator for the WoL Waker plugin.
/// </summary>
public class PluginServiceRegistrator // : IPluginServiceRegistrator
{
    /// <summary>
    /// Registers the plugin services with the dependency injection container.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    public void RegisterServices(IServiceCollection serviceCollection)
    {
        // Register the Wake-on-LAN service
        serviceCollection.AddScoped<WolService>();

        // Register the plugin instance
        serviceCollection.AddScoped<Plugin>();

        // Register the controller
        serviceCollection.AddScoped<Controllers.WolController>();

        // Log service registration
        var logger = serviceCollection.BuildServiceProvider().GetService<ILogger<PluginServiceRegistrator>>();
        logger?.LogInformation("WoL Waker plugin services registered successfully");
    }
}
