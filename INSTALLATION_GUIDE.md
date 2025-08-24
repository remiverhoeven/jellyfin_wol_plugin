# 🚀 WoL Waker Plugin - Installation & Configuration Guide

## 📋 Table of Contents
1. [Overview](#overview)
2. [Installation](#installation)
3. [Configuration](#configuration)
4. [n8n Power Monitoring Setup](#n8n-power-monitoring-setup)
5. [Testing](#testing)
6. [Troubleshooting](#troubleshooting)

## 🎯 Overview

The **WoL Waker** plugin automatically wakes up your archival storage server (Xeon) when users request media from it. It provides:

- **Automatic Wake-on-LAN** when archival media is requested
- **Smart Power Monitoring** via n8n + Zigbee power meter
- **Fallback Monitoring** using network ping/port checks
- **User-Friendly Messages** with progress updates
- **Configurable Timeouts** and retry logic

## 📦 Installation

### Prerequisites
- Jellyfin Server (10.8.0 or later)
- .NET 8.0 Runtime
- Network access to your Xeon server
- Optional: n8n instance for power monitoring

### Step 1: Build the Plugin

#### Option A: Build from Source (Recommended)

**Prerequisites for Building:**
- .NET 8.0 SDK (not just runtime)
- Git
- A code editor (Visual Studio, VS Code, or Rider)

**Build Steps:**

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/yourusername/jellyfin_wol_plugin_new.git
   cd jellyfin_wol_plugin_new
   ```

2. **Build the Plugin:**
   ```bash
   # Navigate to the plugin directory
   cd Jellyfin.Plugin.WolWaker
   
   # Restore NuGet packages
   dotnet restore
   
   # Build the project in Release mode
   dotnet build -c Release
   
   # The DLL will be created in: bin/Release/net8.0/Jellyfin.Plugin.WolWaker.dll
   ```

3. **Create Plugin Directory:**
   ```bash
   # Linux/macOS
   sudo mkdir -p /var/lib/jellyfin/plugins/WoLWaker
   
   # Windows
   mkdir "C:\ProgramData\Jellyfin\Server\plugins\WoLWaker"
   
   # Docker (if using Docker)
   mkdir -p /path/to/jellyfin/config/plugins/WoLWaker
   ```

4. **Copy the Built DLL:**
   ```bash
   # Linux/macOS
   sudo cp bin/Release/net8.0/Jellyfin.Plugin.WolWaker.dll /var/lib/jellyfin/plugins/WoLWaker/
   
   # Windows
   copy "bin\Release\net8.0\Jellyfin.Plugin.WolWaker.dll" "C:\ProgramData\Jellyfin\Server\plugins\WoLWaker\"
   
   # Docker
   cp bin/Release/net8.0/Jellyfin.Plugin.WolWaker.dll /path/to/jellyfin/config/plugins/WoLWaker/
   ```

5. **Verify the Build:**
   ```bash
   # Check if DLL was created
   ls -la bin/Release/net8.0/Jellyfin.Plugin.WolWaker.dll
   
   # Verify file size (should be several hundred KB)
   # Verify file permissions
   ```

#### Option B: Download Pre-built DLL (Alternative)

If you prefer not to build from source:
1. Download `Jellyfin.Plugin.WolWaker.dll` from the releases
2. Create directory: `/path/to/jellyfin/config/plugins/WoLWaker/`
3. Copy the DLL to this directory

**Note:** Building from source ensures you have the latest version and can verify the code.

### Step 2: Restart Jellyfin
```bash
# Systemd (Linux)
sudo systemctl restart jellyfin

# Docker
docker restart jellyfin-container

# Windows Service
Restart-Service Jellyfin
```

### Step 3: Verify Installation
1. Go to Jellyfin Admin → Plugins
2. Look for "WoL Waker" in the list
3. Ensure it shows as "Enabled"

## ⚙️ Configuration

### Accessing Configuration
1. Go to Jellyfin Admin → Plugins
2. Click on "WoL Waker"
3. Click "Configure"

### Essential Settings

#### Wake-on-LAN Configuration
```yaml
MAC Address: 00:11:22:33:44:55          # Your Xeon server's MAC address
Broadcast Address: 255.255.255.255      # Usually 255.255.255.255 for local network
Broadcast Port: 9                       # Standard WoL port
```

#### Server Monitoring
```yaml
Server IP: 192.168.2.17                 # Your Xeon server's IP address
Server Port: 80                         # Port to check for availability (HTTP)
Wake Timeout: 300                       # Maximum wait time in seconds (5 minutes)
Check Interval: 10                      # How often to check server status
```

#### Power Monitoring (Optional)
```yaml
Power Monitoring Enabled: ✓             # Enable/disable power monitoring
Power Monitor API URL: http://192.168.1.100:5678/api/power-status
Power Monitor API Key: [your-api-key]   # Leave empty if no auth required
Power Monitor Poll Interval: 5          # Check power status every 5 seconds
```

#### Behavior Settings
```yaml
Enable Auto Wake: ✓                     # Automatically send WoL packets
Wake On Playback Start: ✓               # Wake server when media is requested
Show User Messages: ✓                   # Display progress to users
Use Fallback Monitoring: ✓              # Use network checks if power monitoring fails
```

#### Safety Settings
```yaml
Cooldown Seconds: 300                   # Wait 5 minutes between wake attempts
Min Wake Interval: 60                   # Minimum 1 minute between attempts
Max Wake Attempts: 3                    # Stop after 3 failed attempts
```

## 🔌 n8n Power Monitoring Setup

### Required API Endpoint Format

Your n8n workflow must expose an endpoint that returns JSON in this format:

```json
{
  "state": "waking",
  "wattage": 45.2,
  "wakeProgress": 25,
  "estimatedTimeToCompletion": 180,
  "status": "Server is starting up",
  "timestamp": "2025-08-24T08:00:00Z"
}
```

### Field Descriptions

| Field | Type | Description | Required |
|-------|------|-------------|----------|
| `state` | string | Power state: `"off"`, `"waking"`, `"running"`, `"idle"` | ✅ |
| `wattage` | number | Current power consumption in watts | ✅ |
| `wakeProgress` | number | Wake-up progress percentage (0-100) | ❌ |
| `estimatedTimeToCompletion` | number | Estimated seconds until ready | ❌ |
| `status` | string | Human-readable status message | ❌ |
| `timestamp` | string | ISO 8601 timestamp | ❌ |

### Power State Values

- **`"off"`**: Server is powered off (0-10W)
- **`"waking"`**: Server is starting up (10-150W)
- **`"running"`**: Server is fully operational (150-200W)
- **`"idle"`**: Server is running but idle (50-100W)

### Example n8n Workflow

```javascript
// In your n8n workflow, create an HTTP endpoint that:
// 1. Reads data from your Zigbee power meter
// 2. Processes the data to determine server state
// 3. Returns the JSON response above

// Example: Power meter reading → Process → HTTP Response
const powerReading = $input.item.json.power; // Your power meter data
const serverState = determineServerState(powerReading);

return {
  state: serverState.state,
  wattage: powerReading,
  wakeProgress: serverState.progress,
  estimatedTimeToCompletion: serverState.eta,
  status: serverState.message,
  timestamp: new Date().toISOString()
};

function determineServerState(powerW) {
  if (powerW <= 10) return { state: "off", progress: 0, eta: 300, message: "Server is off" };
  if (powerW <= 150) return { state: "waking", progress: Math.min(100, (powerW - 10) / 1.4), eta: 180, message: "Server is starting up" };
  if (powerW <= 200) return { state: "running", progress: 100, eta: 0, message: "Server is running" };
  return { state: "idle", progress: 100, eta: 0, message: "Server is idle" };
}
```

### Health Check Endpoint

Also create a health check endpoint at `/health` that returns:

```json
{
  "status": "healthy",
  "service": "power-monitor",
  "timestamp": "2025-08-24T08:00:00Z"
}
```

## 🧪 Testing

### Test 1: Basic Plugin Functionality
```bash
# Test plugin health
curl http://your-jellyfin:8096/wol/health

# Test WoL service
curl http://your-jellyfin:8096/wol/status

# Test configuration
curl http://your-jellyfin:8096/wol/config
```

### Test 2: Power Monitoring
```bash
# Test power monitoring service
curl http://your-jellyfin:8096/wol/power-test

# Test HTTP client
curl http://your-jellyfin:8096/wol/power-http-test
```

### Test 3: Playback Simulation
```bash
# Simulate a playback request for archival media
curl -X POST http://your-jellyfin:8096/wol/playback \
  -H "Content-Type: application/json" \
  -d '{
    "mediaPath": "/mnt/archival/movies/old_movie.mkv",
    "mediaType": "Movie",
    "userId": "test-user-123"
  }'
```

### Test 4: Session Monitoring
```bash
# Check session status
curl http://your-jellyfin:8096/wol/session/[session-id]

# Check user sessions
curl http://your-jellyfin:8096/wol/sessions/[user-id]
```

## 🔧 Troubleshooting

### Common Issues

#### Plugin Not Loading
- Check Jellyfin logs for errors
- Verify .NET 8.0 runtime is installed
- Ensure DLL is in correct directory
- Restart Jellyfin completely

### Build Issues
- **Missing .NET SDK**: Install .NET 8.0 SDK (not just runtime)
- **NuGet restore fails**: Check internet connection and try `dotnet restore --interactive`
- **Build errors**: Ensure all dependencies are compatible with .NET 8.0
- **DLL not found**: Verify the build output path and copy the correct file
- **✅ RESOLVED**: The plugin now builds successfully with modern Jellyfin 10.x packages!

#### Wake-on-LAN Not Working
- Verify MAC address is correct
- Check network broadcast settings
- Ensure WoL is enabled in BIOS
- Test with `wakeonlan` command

#### Power Monitoring Fails
- Check n8n workflow is running
- Verify API endpoint URL is correct
- Test endpoint directly with curl
- Check network connectivity

#### Server Detection Issues
- Verify server IP address
- Check firewall settings
- Test ping and port connectivity
- Ensure server is actually reachable

### Log Analysis

Enable detailed logging in the plugin configuration and check Jellyfin logs for:

```
[INF] WoL Waker plugin initialized successfully!
[INF] Wake-on-LAN packet sent successfully
[INF] Power monitoring service available
[INF] Server is now fully available
[WRN] Power monitoring failed, falling back to network checks
[ERR] Error sending Wake-on-LAN packet
```

### Debug Mode

Set `Enable Detailed Logging: ✓` and `Log Level: Debug` for verbose output.

## ✅ Current Status - Plugin Successfully Built!

**Great News:** The plugin has been successfully updated and now builds with modern Jellyfin 10.x packages! All package dependency issues have been resolved.

### What Was Fixed

1. **✅ Package References Updated**: Replaced outdated packages with modern Jellyfin 10.x equivalents
2. **✅ Plugin Architecture Modernized**: Updated to use current Jellyfin plugin patterns
3. **✅ Build Compatibility**: Plugin now builds successfully with .NET 8.0 and Jellyfin 10.x

### Current Status

- **Build Status**: ✅ Successfully builds in both Debug and Release modes
- **Package Dependencies**: ✅ All resolved and compatible
- **Target Framework**: ✅ .NET 8.0
- **Jellyfin Compatibility**: ✅ Jellyfin 10.x

## 🛠️ Development Setup

### Prerequisites for Development
- .NET 8.0 SDK
- Visual Studio 2022, VS Code with C# extension, or JetBrains Rider
- Git
- Jellyfin development environment (optional, for testing)

### Setting Up Development Environment

1. **Clone and Open:**
   ```bash
   git clone https://github.com/yourusername/jellyfin_wol_plugin_new.git
   cd jellyfin_wol_plugin_new
   ```

2. **Open in Your IDE:**
   ```bash
   # VS Code
   code .
   
   # Visual Studio
   start Jellyfin.Plugin.WolWaker.sln
   ```

3. **Build and Test:**
   ```bash
   # Debug build
   dotnet build
   
   # Run tests (if any)
   dotnet test
   
   # Clean build
   dotnet clean
   dotnet build
   ```

### Project Structure
```
Jellyfin.Plugin.WolWaker/
├── Controllers/          # API endpoints
├── Services/             # Business logic
├── Plugin.cs            # Main plugin entry point
├── PluginConfiguration.cs # Configuration model
└── Web/                 # Frontend assets
```

## 📚 Advanced Configuration

### Custom Media Path Detection

The plugin automatically detects archival media by looking for:
- Paths containing: `archival`, `xeon`, `/mnt/archival`, `/mnt/xeon`
- Network paths starting with: `\\`, `//`
- UNC paths

### Fallback Monitoring

When power monitoring is disabled or fails, the plugin falls back to:
1. **Network Ping**: Check if server responds to ICMP
2. **Port Check**: Verify service is available on configured port
3. **Timeout**: Wait for configured timeout period

### Performance Tuning

- **Power Monitor Poll Interval**: Lower = more responsive, higher = less network traffic
- **Fallback Check Interval**: How often to perform network checks
- **Wake Timeout**: Maximum wait time before giving up

## 🆘 Support

If you encounter issues:

1. Check the troubleshooting section above
2. Enable detailed logging and check Jellyfin logs
3. Test individual components (WoL, power monitoring, network)
4. Verify all configuration values are correct
5. Check network connectivity between all components

## 📝 Configuration Examples

### Minimal Configuration (No Power Monitoring)
```yaml
MAC Address: [your-server-mac]
Server IP: [your-server-ip]
Power Monitoring Enabled: ✗
Use Fallback Monitoring: ✓
Wake Timeout: 300
```

### Full Configuration (With Power Monitoring)
```yaml
MAC Address: [your-server-mac]
Server IP: [your-server-ip]
Power Monitoring Enabled: ✓
Power Monitor API URL: http://[n8n-ip]:5678/api/power-status
Power Monitor Poll Interval: 5
Use Fallback Monitoring: ✓
Fallback Check Interval: 15
Wake Timeout: 300
```

### High-Performance Configuration
```yaml
MAC Address: [your-server-mac]
Server IP: [your-server-ip]
Power Monitoring Enabled: ✓
Power Monitor Poll Interval: 2
Use Fallback Monitoring: ✓
Fallback Check Interval: 5
Wake Timeout: 180
Check Interval: 5
```
