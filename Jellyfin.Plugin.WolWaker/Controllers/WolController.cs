using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Jellyfin.Plugin.WolWaker.Services;

namespace Jellyfin.Plugin.WolWaker.Controllers;

/// <summary>
/// Controller for Wake-on-LAN operations.
/// </summary>
[ApiController]
[Route("wol")]
public class WolController : ControllerBase
{
    private readonly WolService _wolService;
    private readonly Plugin _plugin;
    private readonly ILogger<WolController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WolController"/> class.
    /// </summary>
    /// <param name="wolService">The Wake-on-LAN service.</param>
    /// <param name="plugin">The plugin instance.</param>
    /// <param name="logger">The logger.</param>
    public WolController(WolService wolService, Plugin plugin, ILogger<WolController> logger)
    {
        _wolService = wolService;
        _plugin = plugin;
        _logger = logger;
    }

    /// <summary>
    /// Sends a Wake-on-LAN packet to the configured server.
    /// </summary>
    /// <returns>Result of the wake operation.</returns>
    [HttpPost("wake")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> Wake()
    {
        try
        {
            _logger.LogInformation("Wake-on-LAN request received");

            var success = await _wolService.TrySendAsync(_plugin.Configuration);

            var result = new
            {
                success,
                mac = _plugin.Configuration.MacAddress,
                timestamp = DateTime.UtcNow,
                attemptCount = _wolService.GetAttemptCount(),
                timeSinceLastSent = _wolService.GetTimeSinceLastSent().TotalSeconds
            };

            if (success)
            {
                _logger.LogInformation("Wake-on-LAN packet sent successfully");
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Wake-on-LAN packet not sent (cooldown, interval, or max attempts)");
                return Ok(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Wake-on-LAN packet");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Gets the current status of the Wake-on-LAN service.
    /// </summary>
    /// <returns>Current service status.</returns>
    [HttpGet("status")]
    [AllowAnonymous]
    public ActionResult<object> Status()
    {
        try
        {
            var config = _plugin.Configuration;
            var status = new
            {
                macAddress = config.MacAddress,
                broadcastAddress = config.BroadcastAddress,
                broadcastPort = config.BroadcastPort,
                serverIp = config.ServerIp,
                wakeTimeout = config.WakeTimeout,
                checkInterval = config.CheckInterval,
                enableAutoWake = config.EnableAutoWake,
                showUserMessages = config.ShowUserMessages,
                cooldownSeconds = config.CooldownSeconds,
                powerMonitoringEnabled = config.PowerMonitoringEnabled,
                powerMonitorApiUrl = config.PowerMonitorApiUrl,
                powerMonitorPollInterval = config.PowerMonitorPollInterval,
                wakeOnPlaybackStart = config.WakeOnPlaybackStart,
                minWakeInterval = config.MinWakeInterval,
                maxWakeAttempts = config.MaxWakeAttempts,
                attemptCount = _wolService.GetAttemptCount(),
                timeSinceLastSent = _wolService.GetTimeSinceLastSent().TotalSeconds,
                timestamp = DateTime.UtcNow
            };

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Gets the health status of the plugin.
    /// </summary>
    /// <returns>Plugin health information.</returns>
    [HttpGet("health")]
    [AllowAnonymous]
    public ActionResult<object> Health()
    {
        try
        {
            var health = new
            {
                status = "healthy",
                plugin = "WoL Waker",
                version = "0.1.0",
                timestamp = DateTime.UtcNow,
                configuration = new
                {
                    macAddressConfigured = !string.IsNullOrWhiteSpace(_plugin.Configuration.MacAddress),
                    serverIpConfigured = !string.IsNullOrWhiteSpace(_plugin.Configuration.ServerIp),
                    powerMonitoringEnabled = _plugin.Configuration.PowerMonitoringEnabled
                }
            };

            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting health status");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Tests the Wake-on-LAN functionality.
    /// </summary>
    /// <returns>Test result.</returns>
    [HttpPost("test")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> Test()
    {
        try
        {
            _logger.LogInformation("Wake-on-LAN test request received");

            // Validate configuration
            var config = _plugin.Configuration;
            var validationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(config.MacAddress))
                validationErrors.Add("MAC address not configured");

            if (string.IsNullOrWhiteSpace(config.BroadcastAddress))
                validationErrors.Add("Broadcast address not configured");

            if (string.IsNullOrWhiteSpace(config.ServerIp))
                validationErrors.Add("Server IP not configured");

            if (!WolService.IsValidMacAddress(config.MacAddress))
                validationErrors.Add("Invalid MAC address format");

            if (validationErrors.Any())
            {
                return BadRequest(new
                {
                    success = false,
                    errors = validationErrors,
                    timestamp = DateTime.UtcNow
                });
            }

            // Test Wake-on-LAN packet
            var success = await _wolService.TrySendAsync(config);

            var result = new
            {
                success,
                mac = config.MacAddress,
                broadcastAddress = config.BroadcastAddress,
                broadcastPort = config.BroadcastPort,
                timestamp = DateTime.UtcNow,
                message = success ? "Test packet sent successfully" : "Test packet not sent (cooldown/interval)"
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Wake-on-LAN test");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Resets the Wake-on-LAN attempt counter.
    /// </summary>
    /// <returns>Reset result.</returns>
    [HttpPost("reset")]
    [AllowAnonymous]
    public ActionResult<object> Reset()
    {
        try
        {
            _logger.LogInformation("Wake-on-LAN reset request received");

            _wolService.ResetAttempts();

            var result = new
            {
                success = true,
                message = "Attempt counter reset successfully",
                timestamp = DateTime.UtcNow
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting attempt counter");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Gets the current configuration of the plugin.
    /// </summary>
    /// <returns>Plugin configuration.</returns>
    [HttpGet("config")]
    [AllowAnonymous]
    public ActionResult<object> GetConfig()
    {
        try
        {
            var config = _plugin.Configuration;
            var configData = new
            {
                macAddress = config.MacAddress,
                broadcastAddress = config.BroadcastAddress,
                broadcastPort = config.BroadcastPort,
                serverIp = config.ServerIp,
                wakeTimeout = config.WakeTimeout,
                checkInterval = config.CheckInterval,
                enableAutoWake = config.EnableAutoWake,
                showUserMessages = config.ShowUserMessages,
                cooldownSeconds = config.CooldownSeconds,
                powerMonitoringEnabled = config.PowerMonitoringEnabled,
                powerMonitorApiUrl = config.PowerMonitorApiUrl,
                powerMonitorPollInterval = config.PowerMonitorPollInterval,
                wakeOnPlaybackStart = config.WakeOnPlaybackStart,
                minWakeInterval = config.MinWakeInterval,
                maxWakeAttempts = config.MaxWakeAttempts,
                networkTimeout = config.NetworkTimeout,
                serviceTimeout = config.ServiceTimeout,
                enableDetailedLogging = config.EnableDetailedLogging,
                logLevel = config.LogLevel,
                timestamp = DateTime.UtcNow
            };

            return Ok(configData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configuration");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Gets the configuration page HTML.
    /// </summary>
    /// <returns>HTML configuration page.</returns>
    [HttpGet("config-page")]
    [AllowAnonymous]
    public ActionResult ConfigPage()
    {
        try
        {
            var html = @"<!DOCTYPE html>
<html>
<head>
    <title>WoL Waker Configuration</title>
</head>
<body>
    <h1>WoL Waker Configuration</h1>
    <p>Plugin is working!</p>
    <p>This is a test page to verify Jellyfin can serve the HTML.</p>
    <p>Current MAC: " + _plugin.Configuration.MacAddress + @"</p>
    <p>Current Server IP: " + _plugin.Configuration.ServerIp + @"</p>
</body>
</html>";

            return Content(html, "text/html");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serving configuration page");
            return StatusCode(500, "Error loading configuration page");
        }
    }
}
