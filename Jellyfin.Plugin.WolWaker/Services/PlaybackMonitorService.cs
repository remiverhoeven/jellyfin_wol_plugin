using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Events;
using Jellyfin.Data.Events.System;
using Jellyfin.Data.Events.Users;
using Jellyfin.Plugin.WolWaker.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Playback;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.WolWaker.Services;

/// <summary>
/// Service that monitors Jellyfin playback events and triggers Wake-on-LAN when needed.
/// </summary>
public class PlaybackMonitorService : IDisposable
{
    private readonly ILogger<PlaybackMonitorService> _logger;
    private readonly WolService _wolService;
    private readonly PluginConfiguration _configuration;
    private readonly ILibraryManager _libraryManager;
    private readonly IUserManager _userManager;
    private readonly SemaphoreSlim _wolSemaphore = new SemaphoreSlim(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackMonitorService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="wolService">The Wake-on-LAN service.</param>
    /// <param name="configuration">The plugin configuration.</param>
    /// <param name="libraryManager">The library manager.</param>
    /// <param name="userManager">The user manager.</param>
    public PlaybackMonitorService(
        ILogger<PlaybackMonitorService> logger,
        WolService wolService,
        PluginConfiguration configuration,
        ILibraryManager libraryManager,
        IUserManager userManager)
    {
        _logger = logger;
        _wolService = wolService;
        _configuration = configuration;
        _libraryManager = libraryManager;
        _userManager = userManager;
    }

    /// <summary>
    /// Handles playback start events and triggers WoL if needed.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The playback start event.</param>
    public async Task HandlePlaybackStartAsync(object sender, PlaybackStartEventArgs e)
    {
        if (!_configuration.EnableAutoWake || !_configuration.WakeOnPlaybackStart)
        {
            return;
        }

        try
            {
            _logger.LogInformation("Playback started for item: {ItemName} by user: {UserName}", 
                e.Item?.Name ?? "Unknown", e.User?.Name ?? "Unknown");

            // Check if the item requires archival storage
            if (await RequiresArchivalStorageAsync(e.Item))
            {
                _logger.LogInformation("Item requires archival storage, triggering WoL");
                await TriggerWakeOnLanAsync("Playback start");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling playback start event");
        }
    }

    /// <summary>
    /// Handles library item access events and triggers WoL if needed.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The library item access event.</param>
    public async Task HandleItemAccessAsync(object sender, ItemAccessEventArgs e)
    {
        if (!_configuration.EnableAutoWake)
        {
            return;
        }

        try
        {
            _logger.LogInformation("Item accessed: {ItemName} by user: {UserName}", 
                e.Item?.Name ?? "Unknown", e.User?.Name ?? "Unknown");

            // Check if the item requires archival storage
            if (await RequiresArchivalStorageAsync(e.Item))
            {
                _logger.LogInformation("Item requires archival storage, triggering WoL");
                await TriggerWakeOnLanAsync("Item access");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling item access event");
        }
    }

    /// <summary>
    /// Determines if an item requires archival storage to be available.
    /// </summary>
    /// <param name="item">The library item to check.</param>
    /// <returns>True if archival storage is required; otherwise, false.</returns>
    private async Task<bool> RequiresArchivalStorageAsync(BaseItem? item)
    {
        if (item == null)
        {
            return false;
        }

        try
        {
            // Check if the item's path indicates it's on archival storage
            var itemPath = item.Path;
            if (string.IsNullOrEmpty(itemPath))
            {
                return false;
            }

            // Check if the item is on a network path that matches archival storage patterns
            if (IsArchivalStoragePath(itemPath))
            {
                _logger.LogDebug("Item {ItemName} is on archival storage: {Path}", item.Name, itemPath);
                return true;
            }

            // Check if the item's library is configured as archival
            var library = _libraryManager.GetItemById(item.ParentId);
            if (library != null && IsArchivalLibrary(library))
            {
                _logger.LogDebug("Item {ItemName} belongs to archival library: {LibraryName}", item.Name, library.Name);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if item requires archival storage");
            return false;
        }
    }

    /// <summary>
    /// Determines if a file path is on archival storage.
    /// </summary>
    /// <param name="path">The file path to check.</param>
    /// <returns>True if the path is on archival storage; otherwise, false.</returns>
    private bool IsArchivalStoragePath(string path)
    {
        // Check for common archival storage patterns
        var lowerPath = path.ToLowerInvariant();
        
        // Network paths (UNC)
        if (path.StartsWith("\\\\"))
        {
            // Check if it matches the configured archival server
            if (!string.IsNullOrEmpty(_configuration.ServerIp))
            {
                var serverName = _configuration.ServerIp.Split('.')[0]; // Extract server name from IP
                if (lowerPath.Contains(serverName.ToLowerInvariant()))
                {
                    return true;
                }
            }
            
            // Common archival storage names
            var archivalKeywords = new[] { "archival", "archive", "cold", "storage", "nas", "server" };
            return archivalKeywords.Any(keyword => lowerPath.Contains(keyword));
        }

        // Mounted network drives
        if (path.StartsWith("/mnt/") || path.StartsWith("/media/"))
        {
            var archivalKeywords = new[] { "archival", "archive", "cold", "storage", "nas", "server" };
            return archivalKeywords.Any(keyword => lowerPath.Contains(keyword));
        }

        return false;
    }

    /// <summary>
    /// Determines if a library is configured as archival storage.
    /// </summary>
    /// <param name="library">The library to check.</param>
    /// <returns>True if the library is archival; otherwise, false.</returns>
    private bool IsArchivalLibrary(BaseItem library)
    {
        if (library == null)
        {
            return false;
        }

        var libraryName = library.Name?.ToLowerInvariant() ?? "";
        var archivalKeywords = new[] { "archival", "archive", "cold", "storage", "nas", "server" };
        
        return archivalKeywords.Any(keyword => libraryName.Contains(keyword));
    }

    /// <summary>
    /// Triggers Wake-on-LAN with proper throttling and logging.
    /// </summary>
    /// <param name="triggerReason">The reason for triggering WoL.</param>
    private async Task TriggerWakeOnLanAsync(string triggerReason)
    {
        try
        {
            // Use semaphore to prevent multiple simultaneous WoL attempts
            if (!await _wolSemaphore.WaitAsync(TimeSpan.FromSeconds(5)))
            {
                _logger.LogWarning("WoL already in progress, skipping request for: {Reason}", triggerReason);
                return;
            }

            try
            {
                _logger.LogInformation("Triggering WoL due to: {Reason}", triggerReason);
                
                var success = await _wolService.TrySendAsync(_configuration);
                
                if (success)
                {
                    _logger.LogInformation("WoL triggered successfully for: {Reason}", triggerReason);
                    
                    if (_configuration.ShowUserMessages)
                    {
                        // TODO: Show user notification that archival storage is being woken up
                        _logger.LogInformation("User notification: Archival storage is being woken up");
                    }
                }
                else
                {
                    _logger.LogWarning("WoL failed for: {Reason}", triggerReason);
                }
            }
            finally
            {
                _wolSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering WoL for: {Reason}", triggerReason);
        }
    }

    /// <summary>
    /// Disposes the service.
    /// </summary>
    public void Dispose()
    {
        _wolSemaphore?.Dispose();
    }
}
