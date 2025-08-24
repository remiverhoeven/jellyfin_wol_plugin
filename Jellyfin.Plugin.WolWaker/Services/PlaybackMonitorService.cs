using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.WolWaker;
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
    private readonly SemaphoreSlim _wolSemaphore = new SemaphoreSlim(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackMonitorService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="wolService">The Wake-on-LAN service.</param>
    /// <param name="configuration">The plugin configuration.</param>
    public PlaybackMonitorService(
        ILogger<PlaybackMonitorService> logger,
        WolService wolService,
        PluginConfiguration configuration)
    {
        _logger = logger;
        _wolService = wolService;
        _configuration = configuration;
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
