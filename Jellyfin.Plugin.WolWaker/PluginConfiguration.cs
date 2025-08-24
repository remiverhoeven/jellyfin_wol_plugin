using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.WolWaker;

/// <summary>
/// Configuration class for the WoL Waker plugin.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets or sets the MAC address of the target server for Wake-on-LAN.
    /// </summary>
    public string MacAddress { get; set; } = "00:11:22:33:44:55";

    /// <summary>
    /// Gets or sets the broadcast address for Wake-on-LAN packets.
    /// </summary>
    public string BroadcastAddress { get; set; } = "255.255.255.255";

    /// <summary>
    /// Gets or sets the broadcast port for Wake-on-LAN packets.
    /// </summary>
    public int BroadcastPort { get; set; } = 9;

    /// <summary>
    /// Gets or sets the IP address of the target server.
    /// </summary>
    public string ServerIp { get; set; } = "192.168.2.17";

    /// <summary>
    /// Gets or sets the maximum time to wait for server wake-up in seconds.
    /// </summary>
    public int WakeTimeout { get; set; } = 300;

    /// <summary>
    /// Gets or sets the interval between server status checks in seconds.
    /// </summary>
    public int CheckInterval { get; set; } = 10;

    /// <summary>
    /// Gets or sets a value indicating whether automatic wake-up is enabled.
    /// </summary>
    public bool EnableAutoWake { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether user messages are shown.
    /// </summary>
    public bool ShowUserMessages { get; set; } = true;

    /// <summary>
    /// Gets or sets the cooldown period between wake attempts in seconds.
    /// </summary>
    public int CooldownSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets a value indicating whether power monitoring is enabled.
    /// </summary>
    public bool PowerMonitoringEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the URL for the power monitoring API.
    /// </summary>
    public string PowerMonitorApiUrl { get; set; } = "http://192.168.2.226:5678/api/power-status";

    /// <summary>
    /// Gets or sets the polling interval for power monitoring in seconds.
    /// </summary>
    public int PowerMonitorPollInterval { get; set; } = 5;

    /// <summary>
    /// Gets or sets the API key for power monitoring (if required).
    /// </summary>
    public string PowerMonitorApiKey { get; set; } = "";

    /// <summary>
    /// Gets or sets a value indicating whether to wake on first API hit.
    /// </summary>
    public bool WakeOnFirstApiHit { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to wake on playback start.
    /// </summary>
    public bool WakeOnPlaybackStart { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum number of seconds between wake attempts.
    /// </summary>
    public int MinWakeInterval { get; set; } = 60;

    /// <summary>
    /// Gets or sets the maximum number of wake attempts before giving up.
    /// </summary>
    public int MaxWakeAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the timeout for network connectivity checks in seconds.
    /// </summary>
    public int NetworkTimeout { get; set; } = 5;

    /// <summary>
    /// Gets or sets the timeout for service availability checks in seconds.
    /// </summary>
    public int ServiceTimeout { get; set; } = 10;

    /// <summary>
    /// Gets or sets a value indicating whether to log all operations.
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets the log level for the plugin.
    /// </summary>
    public string LogLevel { get; set; } = "Information";
}
