# Jellyfin Wake-on-LAN Plugin

A Jellyfin plugin that automatically wakes up archival storage servers when media is requested, enabling power-efficient operation in multi-server media ecosystems.

## Overview

This plugin is designed for setups where you have:
- **Primary Server** (N100): Always-on Jellyfin instance with local media
- **Storage Server** (Xeon): On-demand server with archival media storage
- **Goal**: Minimize power consumption while maintaining seamless user experience

## Features

- **Automatic Wake-on-LAN**: Sends magic packets when archival media is requested
- **User Experience**: Displays "please wait" message during server startup
- **Seamless Playback**: Automatically continues media playback once server is online
- **Power Efficiency**: Only wakes servers when actually needed
- **Configurable**: Easy setup and customization options

## Architecture

```
User Request → Plugin Intercepts → Send WoL → Monitor Server → Resume Playback
     ↓              ↓              ↓           ↓           ↓
  Media from    Detect Archival   Magic     Health      Continue
  Xeon Server   Storage Request   Packet    Checks      Playback
```

## Requirements

- Jellyfin Server 10.x
- .NET 8.0 Runtime
- Network access to target server
- Wake-on-LAN enabled on target server

## Installation

### Prerequisites

1. **Install .NET 8.0 SDK** (for development) or Runtime (for production)
2. **Enable Wake-on-LAN** on your target server (Xeon)
3. **Verify network connectivity** between Jellyfin server and target

### Build from Source

```bash
# Clone the repository
git clone https://github.com/remiverhoeven/truenas_scripts.git
cd truenas_scripts/jellyfin_wol_plugin/Jellyfin.Plugin.WolWaker

# Build the plugin
dotnet restore
dotnet build -c Release

# Copy to Jellyfin plugins directory
cp bin/Release/net8.0/Jellyfin.Plugin.WolWaker.dll /var/lib/jellyfin/plugins/WoLWaker/
```

### Configuration

1. **Restart Jellyfin** after installing the plugin
2. **Navigate to** Dashboard → Plugins → WoL Waker
3. **Configure settings**:
   - **MAC Address**: Target server's Wake-on-LAN interface MAC
   - **Broadcast Address**: Network broadcast address (usually 255.255.255.255)
   - **Server IP**: Target server's main interface IP
   - **Wake Timeout**: Maximum time to wait for server (default: 300s)

## Usage

### Automatic Operation

Once configured, the plugin will:
1. **Intercept** media requests for archival storage
2. **Send** Wake-on-LAN packet to target server
3. **Display** status message to user
4. **Monitor** server availability
5. **Resume** playback automatically

### Manual Control

You can also manually trigger wake operations:
- **API Endpoint**: `GET /wol/wake`
- **Status Check**: `GET /wol/status`
- **Health Check**: `GET /wol/health`

### Example API Usage

```bash
# Wake the server
curl http://your-jellyfin:8096/wol/wake

# Check status
curl http://your-jellyfin:8096/wol/status

# Test configuration
curl http://your-jellyfin:8096/wol/test
```

## Configuration Options

| Setting | Description | Default | Required |
|---------|-------------|---------|----------|
| `MacAddress` | Target server MAC address | - | Yes |
| `BroadcastAddress` | Network broadcast address | 255.255.255.255 | Yes |
| `ServerIp` | Target server IP address | - | Yes |
| `WakeTimeout` | Maximum wait time (seconds) | 300 | No |
| `CheckInterval` | Health check interval (seconds) | 10 | No |
| `EnableAutoWake` | Enable automatic wake | true | No |
| `ShowUserMessages` | Show status messages | true | No |
| `CooldownSeconds` | Minimum time between wakes | 300 | No |

## Troubleshooting

### Common Issues

#### Wake-on-LAN Not Working
1. **Check BIOS settings** on target server
2. **Verify MAC address** is correct
3. **Check network connectivity**
4. **Review firewall settings**

#### Plugin Not Loading
1. **Verify .NET 8.0** is installed
2. **Check file permissions** in plugins directory
3. **Review Jellyfin logs** for errors
4. **Restart Jellyfin** service

#### Server Not Waking
1. **Test network connectivity** to target
2. **Verify Wake-on-LAN** is enabled
3. **Check target server** power state
4. **Review network configuration**

### Logs

Plugin logs are available in:
- **Jellyfin logs**: Check Jellyfin dashboard for plugin logs
- **System logs**: `/var/log/jellyfin/` (Linux) or Event Viewer (Windows)

### Debug Mode

Enable debug logging in Jellyfin dashboard:
1. **Navigate to** Dashboard → Advanced → Logging
2. **Set Log Level** to Debug
3. **Restart Jellyfin**

## Development

### Project Structure

```
Jellyfin.Plugin.WolWaker/
├── Plugin.cs                    # Main plugin class
├── PluginConfiguration.cs       # Configuration management
├── WolService.cs               # Wake-on-LAN implementation
├── WolController.cs            # HTTP API endpoints
├── PlaybackInterceptor.cs      # Media request interception
├── ServerMonitor.cs            # Server status monitoring
├── UserInterface.cs            # Frontend integration
├── StartupEntry.cs             # Plugin initialization
└── Web/                        # Configuration UI
    └── wolwaker.html
```

### Building

```bash
# Development build
dotnet build

# Release build
dotnet build -c Release

# Run tests
dotnet test

# Clean build artifacts
dotnet clean
```

### Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/unit_tests/

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Contributing

1. **Fork** the repository
2. **Create** a feature branch
3. **Make** your changes
4. **Add** tests for new functionality
5. **Submit** a pull request

## License

This project is licensed under the GPL-3.0 License - see the [LICENSE](LICENSE) file for details.

## Support

- **Issues**: [GitHub Issues](https://github.com/remiverhoeven/truenas_scripts/issues)
- **Discussions**: [GitHub Discussions](https://github.com/remiverhoeven/truenas_scripts/discussions)
- **Documentation**: See [docs/](docs/) directory

## Acknowledgments

- **Jellyfin Team** for the excellent plugin architecture
- **Community** for feedback and testing
- **Open Source** contributors who made this possible

---

**Version**: 0.1.0  
**Last Updated**: December 2024  
**Jellyfin Compatibility**: 10.x  
**Platform**: Cross-platform (.NET 8.0)
