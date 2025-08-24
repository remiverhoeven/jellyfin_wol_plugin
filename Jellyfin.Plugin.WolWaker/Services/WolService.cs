using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.WolWaker.Services;

/// <summary>
/// Service for sending Wake-on-LAN magic packets.
/// </summary>
public class WolService
{
    private readonly ILogger<WolService> _logger;
    private DateTime _lastSent = DateTime.MinValue;
    private int _attemptCount = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="WolService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public WolService(ILogger<WolService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Attempts to send a Wake-on-LAN packet to the specified MAC address.
    /// </summary>
    /// <param name="configuration">The plugin configuration.</param>
    /// <returns>True if the packet was sent successfully; otherwise, false.</returns>
    public async Task<bool> TrySendAsync(PluginConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration.MacAddress))
        {
            _logger.LogWarning("WoL not sent: MAC address not configured");
            return false;
        }

        // Check cooldown
        if (configuration.CooldownSeconds > 0 && 
            (DateTime.UtcNow - _lastSent).TotalSeconds < configuration.CooldownSeconds)
        {
            _logger.LogInformation("WoL suppressed by cooldown ({CooldownSeconds}s remaining)", 
                configuration.CooldownSeconds - (int)(DateTime.UtcNow - _lastSent).TotalSeconds);
            return false;
        }

        // Check minimum interval
        if (configuration.MinWakeInterval > 0 && 
            (DateTime.UtcNow - _lastSent).TotalSeconds < configuration.MinWakeInterval)
        {
            _logger.LogInformation("WoL suppressed by minimum interval ({MinInterval}s remaining)", 
                configuration.MinWakeInterval - (int)(DateTime.UtcNow - _lastSent).TotalSeconds);
            return false;
        }

        // Check maximum attempts
        if (configuration.MaxWakeAttempts > 0 && _attemptCount >= configuration.MaxWakeAttempts)
        {
            _logger.LogWarning("WoL not sent: Maximum attempts ({MaxAttempts}) reached", configuration.MaxWakeAttempts);
            return false;
        }

        try
        {
            var macBytes = ParseMacAddress(configuration.MacAddress);
            var magicPacket = CreateMagicPacket(macBytes);

            using var client = new UdpClient();
            client.EnableBroadcast = true;

            var endpoint = new IPEndPoint(
                IPAddress.Parse(configuration.BroadcastAddress), 
                configuration.BroadcastPort);

            await client.SendAsync(magicPacket, magicPacket.Length, endpoint);

            _lastSent = DateTime.UtcNow;
            _attemptCount++;

            _logger.LogInformation(
                "WoL magic packet sent to {Mac} via {BroadcastAddress}:{BroadcastPort} (attempt {AttemptCount})", 
                configuration.MacAddress, 
                configuration.BroadcastAddress, 
                configuration.BroadcastPort,
                _attemptCount);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Wake-on-LAN packet to {Mac}", configuration.MacAddress);
            return false;
        }
    }

    /// <summary>
    /// Resets the attempt counter.
    /// </summary>
    public void ResetAttempts()
    {
        _attemptCount = 0;
        _logger.LogInformation("WoL attempt counter reset");
    }

    /// <summary>
    /// Gets the time since the last Wake-on-LAN packet was sent.
    /// </summary>
    /// <returns>TimeSpan since last packet.</returns>
    public TimeSpan GetTimeSinceLastSent()
    {
        return DateTime.UtcNow - _lastSent;
    }

    /// <summary>
    /// Gets the current attempt count.
    /// </summary>
    /// <returns>Number of attempts made.</returns>
    public int GetAttemptCount()
    {
        return _attemptCount;
    }

    /// <summary>
    /// Creates a Wake-on-LAN magic packet.
    /// </summary>
    /// <param name="macBytes">The MAC address as bytes.</param>
    /// <returns>The magic packet as bytes.</returns>
    private static byte[] CreateMagicPacket(byte[] macBytes)
    {
        // Magic packet: 6 bytes of 0xFF followed by MAC address repeated 16 times
        var packet = new byte[6 + 16 * 6];
        
        // Fill first 6 bytes with 0xFF
        for (int i = 0; i < 6; i++)
        {
            packet[i] = 0xFF;
        }

        // Repeat MAC address 16 times
        for (int i = 6; i < packet.Length; i += 6)
        {
            Buffer.BlockCopy(macBytes, 0, packet, i, 6);
        }

        return packet;
    }

    /// <summary>
    /// Parses a MAC address string into bytes.
    /// </summary>
    /// <param name="macAddress">The MAC address string.</param>
    /// <returns>The MAC address as bytes.</returns>
    /// <exception cref="FormatException">Thrown when the MAC address format is invalid.</exception>
    private static byte[] ParseMacAddress(string macAddress)
    {
        if (string.IsNullOrWhiteSpace(macAddress))
        {
            throw new FormatException("MAC address cannot be null or empty");
        }

        // Remove common separators and convert to uppercase
        var cleanMac = macAddress.Replace(":", "").Replace("-", "").Replace(".", "").ToUpper();

        if (cleanMac.Length != 12)
        {
            throw new FormatException($"Invalid MAC address length: {cleanMac.Length} (expected 12 characters)");
        }

        // Validate hex characters
        if (!cleanMac.All(c => "0123456789ABCDEF".Contains(c)))
        {
            throw new FormatException("MAC address contains invalid characters");
        }

        var bytes = new byte[6];
        for (int i = 0; i < 6; i++)
        {
            bytes[i] = Convert.ToByte(cleanMac.Substring(i * 2, 2), 16);
        }

        return bytes;
    }

    /// <summary>
    /// Validates a MAC address format.
    /// </summary>
    /// <param name="macAddress">The MAC address to validate.</param>
    /// <returns>True if the MAC address is valid; otherwise, false.</returns>
    public static bool IsValidMacAddress(string macAddress)
    {
        try
        {
            ParseMacAddress(macAddress);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
