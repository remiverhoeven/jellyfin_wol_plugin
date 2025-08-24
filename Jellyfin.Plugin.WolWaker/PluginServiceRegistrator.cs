using System;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Jellyfin.Plugin.WolWaker.Services;

namespace Jellyfin.Plugin.WolWaker;

/// <summary>
/// Service registrator for the WoL Waker plugin.
/// </summary>
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <summary>
    /// Registers the plugin services with the dependency injection container.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="applicationHost">The application host.</param>
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        // Register the Wake-on-LAN service
        serviceCollection.AddSingleton<WolService>();

        // Register the plugin instance (as a singleton, since it's the main plugin instance)
        serviceCollection.AddSingleton<Plugin>();

        // Register the controller (scoped, as it's typically per-request)
        serviceCollection.AddScoped<Controllers.WolController>();

        // Logging for service registration is usually handled by the plugin manager itself
        // or within the services/controllers that have injected loggers.
    }
}
