# Development Guide: Jellyfin Wake-on-LAN Plugin

## Local Development Setup

### Prerequisites

- **.NET 8.0 SDK** - For building the plugin
- **Jellyfin Server** - Local instance for testing
- **Git** - Version control
- **IDE** - Visual Studio Code, Visual Studio, or Rider

### Local Jellyfin Installation

#### Option 1: macOS (Homebrew)
```bash
# Install Jellyfin
brew install jellyfin

# Start Jellyfin service
brew services start jellyfin

# Access at http://localhost:8096
```

#### Option 2: Docker (Recommended)
```bash
# Create test Jellyfin container
docker run -d \
  --name jellyfin-dev \
  -p 8096:8096 \
  -v ~/jellyfin-dev-config:/config \
  -v ~/test-media:/media \
  jellyfin/jellyfin:latest

# Access at http://localhost:8096
```

#### Option 3: Direct Download
1. Download from [jellyfin.org/downloads](https://jellyfin.org/downloads/)
2. Extract and run locally
3. Configure for development

### Development Environment Setup

#### 1. Clone and Setup
```bash
git clone https://github.com/remiverhoeven/truenas_scripts.git
cd truenas_scripts/jellyfin_wol_plugin/Jellyfin.Plugin.WolWaker
```

#### 2. Install Dependencies
```bash
dotnet restore
dotnet build
```

#### 3. Configure Local Jellyfin
1. **Start Jellyfin** and complete initial setup
2. **Create test libraries** with sample media
3. **Note the data directory** for plugin installation

#### 4. Install Plugin for Development
```bash
# Build in debug mode
dotnet build -c Debug

# Copy to Jellyfin plugins directory
# macOS: ~/Library/Application Support/Jellyfin-Server/plugins/
# Linux: ~/.local/share/jellyfin/plugins/
# Windows: %APPDATA%\Jellyfin-Server\plugins\

# Create plugin directory
mkdir -p ~/Library/Application\ Support/Jellyfin-Server/plugins/WoLWaker/

# Copy plugin files
cp bin/Debug/net8.0/Jellyfin.Plugin.WolWaker.dll ~/Library/Application\ Support/Jellyfin-Server/plugins/WoLWaker/
```

## Testing Strategy

### Local Testing Scenarios

#### 1. **Basic Plugin Loading**
- [ ] Plugin appears in Jellyfin dashboard
- [ ] Configuration page loads correctly
- [ ] No errors in Jellyfin logs

#### 2. **Wake-on-LAN Functionality**
- [ ] WoL packet sent successfully
- [ ] Configuration validation works
- [ ] API endpoints respond correctly

#### 3. **Media Interception** (Simulated)
- [ ] Plugin detects media requests
- [ ] Status messages display correctly
- [ ] User interface integration works

#### 4. **Power Monitoring Integration**
- [ ] n8n API calls work correctly
- [ ] Power data parsing functions
- [ ] Progress tracking updates

### Mock Services for Local Testing

#### Power Monitoring Mock
```csharp
public class MockPowerMonitorService : IPowerMonitorService
{
    public async Task<PowerStatus> GetPowerStatusAsync()
    {
        // Simulate power consumption patterns
        var random = new Random();
        var wattage = random.Next(5, 200);
        
        return new PowerStatus
        {
            CurrentWattage = wattage,
            IsWakingUp = wattage > 10,
            LastUpdate = DateTime.UtcNow,
            BaselineWattage = 5,
            FullPowerWattage = 180
        };
    }
}
```

#### Network Monitoring Mock
```csharp
public class MockNetworkMonitor
{
    public async Task<bool> IsServerReachableAsync(string ip)
    {
        // Simulate network connectivity
        await Task.Delay(100); // Simulate network delay
        return true; // Always return true for local testing
    }
}
```

## TrueNAS Scale Deployment

### Production Environment Differences

#### 1. **File Paths**
- **Local**: `~/Library/Application Support/Jellyfin-Server/plugins/`
- **TrueNAS**: `/var/lib/jellyfin/plugins/`

#### 2. **Permissions**
- **Local**: User permissions
- **TrueNAS**: System service permissions

#### 3. **Network Configuration**
- **Local**: Localhost/127.0.0.1
- **TrueNAS**: Actual network IPs (192.168.2.x)

#### 4. **Media Paths**
- **Local**: Local test media
- **TrueNAS**: NFS-mounted Xeon storage

### Deployment Checklist

#### Pre-Deployment
- [ ] Plugin tested locally
- [ ] Configuration validated
- [ ] Network settings confirmed
- [ ] Power monitoring API accessible

#### Deployment Steps
1. **Build release version**
   ```bash
   dotnet build -c Release
   ```

2. **Copy to TrueNAS**
   ```bash
   # Create plugin directory
   sudo mkdir -p /var/lib/jellyfin/plugins/WoLWaker/
   
   # Copy plugin files
   sudo cp bin/Release/net8.0/Jellyfin.Plugin.WolWaker.dll /var/lib/jellyfin/plugins/WoLWaker/
   
   # Set permissions
   sudo chown -R jellyfin:jellyfin /var/lib/jellyfin/plugins/WoLWaker/
   sudo chmod -R 755 /var/lib/jellyfin/plugins/WoLWaker/
   ```

3. **Restart Jellyfin service**
   ```bash
   sudo systemctl restart jellyfin
   ```

4. **Verify installation**
   - Check Jellyfin dashboard → Plugins
   - Verify plugin appears and loads
   - Check logs for any errors

## Configuration Differences

### Local Development
```json
{
  "MacAddress": "00:11:22:33:44:55",
  "BroadcastAddress": "255.255.255.255",
  "ServerIp": "127.0.0.1",
  "PowerMonitorApiUrl": "http://localhost:5678/api/power-status",
  "PowerMonitoringEnabled": true
}
```

### TrueNAS Production
```json
{
  "MacAddress": "a4:ae:11:19:ac:33",
  "BroadcastAddress": "192.168.2.255",
  "ServerIp": "192.168.2.17",
  "PowerMonitorApiUrl": "http://192.168.2.226:5678/api/power-status",
  "PowerMonitoringEnabled": true
}
```

## Debugging and Troubleshooting

### Local Development
- **Jellyfin Logs**: Check Jellyfin dashboard → Advanced → Logs
- **Plugin Logs**: Look for plugin-specific log entries
- **Debug Mode**: Enable debug logging in Jellyfin

### TrueNAS Production
- **System Logs**: `sudo journalctl -u jellyfin -f`
- **Plugin Logs**: Check Jellyfin dashboard logs
- **Network Debugging**: Test connectivity between N100 and Xeon

### Common Issues

#### Plugin Not Loading
1. Check file permissions
2. Verify .NET runtime compatibility
3. Check Jellyfin logs for errors
4. Confirm plugin directory structure

#### Wake-on-LAN Not Working
1. Verify MAC address configuration
2. Check network connectivity
3. Test WoL from command line
4. Verify BIOS settings on target server

#### Power Monitoring Issues
1. Test n8n API endpoint directly
2. Check network connectivity to n8n
3. Verify API response format
4. Check n8n workflow logs

## Development Workflow

### Daily Development Cycle
1. **Make changes** to plugin code
2. **Build locally** with `dotnet build`
3. **Copy to local Jellyfin** plugins directory
4. **Test functionality** in local Jellyfin instance
5. **Debug and iterate** as needed
6. **Commit changes** to Git repository

### Release Preparation
1. **Complete local testing** of all features
2. **Update version numbers** in project files
3. **Build release version** with `dotnet build -c Release`
4. **Test release build** locally
5. **Deploy to TrueNAS** for production testing
6. **Monitor production** for any issues

## Performance Considerations

### Local Development
- **Resource Usage**: Minimal impact on development machine
- **Network**: Local network only
- **Storage**: Local test media

### TrueNAS Production
- **Resource Usage**: Monitor plugin impact on Jellyfin performance
- **Network**: Real network latency and bandwidth
- **Storage**: NFS performance and reliability

---

**Document Version**: 1.0  
**Last Updated**: December 2024  
**Environment**: Local Development + TrueNAS Production
