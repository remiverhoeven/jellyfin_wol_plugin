using System;
using System.Collections.Generic;
using System.Globalization;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.WolWaker;

/// <summary>
/// Main plugin class for the Jellyfin Wake-on-LAN Plugin.
/// This plugin automatically wakes up archival storage servers when media is requested.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// Gets the plugin name.
    /// </summary>
    public override string Name => "WoL Waker";

    /// <summary>
    /// Gets the plugin ID.
    /// </summary>
    public override Guid Id => Guid.Parse("0ee23b2e-9d4d-4b5e-a0b9-7b4e54c5a5f2");

    /// <summary>
    /// Gets the plugin description.
    /// </summary>
    public override string Description => "Automatically wakes up archival storage servers when media is requested, enabling power-efficient operation.";

    /// <summary>
    /// Gets the plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="appPaths">The application paths.</param>
    /// <param name="xmlSerializer">The XML serializer.</param>
    /// <param name="logger">The logger.</param>
    public Plugin(IApplicationPaths appPaths, MediaBrowser.Model.Serialization.IXmlSerializer xmlSerializer, ILogger<Plugin> logger)
        : base(appPaths, xmlSerializer)
    {
        Instance = this;
        logger.LogInformation("WoL Waker plugin initialized");
    }

    /// <summary>
    /// Gets the plugin web pages.
    /// </summary>
    /// <returns>Collection of plugin page information.</returns>
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            new PluginPageInfo
            {
                // This is the route in the Dashboard UI:
                // #/configurationpage?name=WoLWaker
                Name = "WoLWaker",
                // This must exactly match your assembly namespace + path
                EmbeddedResourcePath = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}.Web.wolwaker.html",
                    GetType().Namespace)
            }
        };
    }
}
