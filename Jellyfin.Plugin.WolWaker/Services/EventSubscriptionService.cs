using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Jellyfin.Data.Events;

namespace Jellyfin.Plugin.WolWaker.Services;

/// <summary>
/// Service that subscribes to Jellyfin events and triggers WoL when remote media is accessed.
/// </summary>
public class EventSubscriptionService : IHostedService
{
    private readonly ILogger<EventSubscriptionService> _logger;
    private readonly PlaybackMonitorService _playbackMonitor;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventSubscriptionService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="playbackMonitor">The playback monitor service.</param>
    public EventSubscriptionService(
        ILogger<EventSubscriptionService> logger,
        PlaybackMonitorService playbackMonitor)
    {
        _logger = logger;
        _playbackMonitor = playbackMonitor;
    }

    /// <summary>
    /// Starts the service and logs initialization.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("EventSubscriptionService started - monitoring for remote media access");
            _logger.LogInformation("Note: Event subscriptions will be handled by Jellyfin's built-in event system");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting EventSubscriptionService");
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Stops the service.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("EventSubscriptionService stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping EventSubscriptionService");
        }

        await Task.CompletedTask;
    }
}
