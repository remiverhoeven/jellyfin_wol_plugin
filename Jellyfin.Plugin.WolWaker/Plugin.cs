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
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="appPaths">The application paths.</param>
    /// <param name="logger">The logger.</param>
    public Plugin(IApplicationPaths appPaths, ILogger<Plugin> logger)
        : base(appPaths, logger)
    {
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
                Name = "wolwaker",
                EmbeddedResourcePath = GetType().Namespace + ".Web.wolwaker.html"
            }
        };
    }
}
