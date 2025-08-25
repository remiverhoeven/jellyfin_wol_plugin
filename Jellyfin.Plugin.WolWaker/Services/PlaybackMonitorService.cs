using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.WolWaker;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Jellyfin.Data.Events;
using MediaBrowser.Controller.Entities;
using System.IO;

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
    private readonly SemaphoreSlim _wolSemaphore = new SemaphoreSlim(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackMonitorService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="wolService">The Wake-on-LAN service.</param>
    /// <param name="configuration">The plugin configuration.</param>
    /// <param name="libraryManager">The library manager.</param>
    public PlaybackMonitorService(
        ILogger<PlaybackMonitorService> logger,
        WolService wolService,
        PluginConfiguration configuration,
        ILibraryManager libraryManager)
    {
        _logger = logger;
        _wolService = wolService;
        _configuration = configuration;
        _libraryManager = libraryManager;
        
        _logger.LogInformation("PlaybackMonitorService initialized - monitoring for remote media access");
    }

    /// <summary>
    /// Handles playback start events and checks if remote media is being accessed.
    /// </summary>
    /// <param name="eventArgs">The playback start event arguments.</param>
    public async Task HandlePlaybackStartAsync(PlaybackStartEventArgs eventArgs)
    {
        try
        {
            var item = eventArgs.Item;
            if (item == null) return;

            _logger.LogDebug("Playback started for item: {ItemName} ({ItemPath})", item.Name, item.Path);

            // Check if this is remote media that needs WoL
            if (IsRemoteMedia(item))
            {
                _logger.LogInformation("Remote media playback detected: {ItemName}", item.Name);
                await TriggerWakeOnLanAsync($"Remote media playback: {item.Name}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling playback start event");
        }
    }

    /// <summary>
    /// Handles item access events to detect when remote media is requested.
    /// </summary>
    /// <param name="eventArgs">The item access event arguments.</param>
    public async Task HandleItemAccessAsync(GenericEventArgs<BaseItem> eventArgs)
    {
        try
        {
            var item = eventArgs.Argument;
            if (item == null) return;

            _logger.LogDebug("Item accessed: {ItemName} ({ItemPath})", item.Name, item.Path);

            // Check if this is remote media that needs WoL
            if (IsRemoteMedia(item))
            {
                _logger.LogInformation("Remote media access detected: {ItemName}", item.Name);
                await TriggerWakeOnLanAsync($"Remote media access: {item.Name}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling item access event");
        }
    }

    /// <summary>
    /// Checks if an item is remote media that requires WoL.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>True if the item is remote media that needs WoL.</returns>
    private bool IsRemoteMedia(MediaBrowser.Controller.Entities.BaseItem item)
    {
        try
        {
            if (string.IsNullOrEmpty(item.Path)) return false;

            // Check if the path contains remote media indicators
            var path = item.Path.ToLowerInvariant();
            
            // Check for remote storage paths
            if (path.Contains("/media/remote/") || 
                path.Contains("\\media\\remote\\") ||
                path.Contains("xeon_media") ||
                path.Contains("remote"))
            {
                _logger.LogDebug("Item identified as remote media: {Path}", item.Path);
                return true;
            }

            // Check if the item's library is remote
            var library = _libraryManager.GetItemById(item.Id);
            if (library != null)
            {
                var libraryPath = library.Path?.ToLowerInvariant() ?? "";
                if (libraryPath.Contains("/media/remote/") || 
                    libraryPath.Contains("\\media\\remote\\") ||
                    libraryPath.Contains("xeon_media") ||
                    libraryPath.Contains("remote"))
                {
                    _logger.LogDebug("Item library identified as remote: {LibraryPath}", library.Path);
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if item is remote media");
            return false;
        }
    }

    /// <summary>
    /// Checks if a file path indicates remote media access.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns>True if the path indicates remote media access.</returns>
    public bool IsRemoteMediaPath(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath)) return false;

            var path = filePath.ToLowerInvariant();
            
            // Check for remote storage paths
            if (path.Contains("/media/remote/") || 
                path.Contains("\\media\\remote\\") ||
                path.Contains("xeon_media") ||
                path.Contains("remote"))
            {
                _logger.LogDebug("Path identified as remote media: {Path}", filePath);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if path is remote media");
            return false;
        }
    }

    /// <summary>
    /// Triggers WoL when remote media access is detected.
    /// </summary>
    /// <param name="filePath">The file path that was accessed.</param>
    public async Task CheckAndTriggerWolAsync(string filePath)
    {
        try
        {
            if (IsRemoteMediaPath(filePath))
            {
                _logger.LogInformation("Remote media access detected: {Path}", filePath);
                await TriggerWakeOnLanAsync($"Remote media access: {Path.GetFileName(filePath)}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking and triggering WoL for path: {Path}", filePath);
        }
    }

    /// <summary>
    /// Triggers Wake-on-LAN manually (can be called from API endpoints).
    /// </summary>
    /// <param name="triggerReason">The reason for triggering WoL.</param>
    public async Task TriggerWakeOnLanAsync(string triggerReason)
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
