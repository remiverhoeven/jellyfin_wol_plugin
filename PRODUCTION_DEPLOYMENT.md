# 🚀 Production Deployment Checklist

## 📋 Pre-Deployment Checklist

Before deploying to your TrueNAS Scale production environment, ensure you have:

### ✅ System Requirements
- [ ] **Jellyfin Server**: Running on TrueNAS Scale (10.8.0 or later)
- [ ] **.NET Runtime**: .NET 8.0 runtime installed
- [ ] **Network Access**: Jellyfin can reach your Xeon server and n8n instance
- [ ] **WoL Enabled**: Wake-on-LAN enabled in your Xeon server's BIOS

### ✅ Network Information
- [ ] **Xeon Server MAC Address**: `XX:XX:XX:XX:XX:XX`
- [ ] **Xeon Server IP Address**: `192.168.x.x`
- [ ] **Xeon Server Port**: Port to check for availability (80 for HTTP, 22 for SSH)
- [ ] **n8n IP Address**: `192.168.x.x` (if using power monitoring)
- [ ] **n8n Port**: Usually `5678`
- [ ] **Network Broadcast**: Usually `255.255.255.255`

### ✅ n8n Setup (Optional)
- [ ] **n8n Instance**: Running and accessible
- [ ] **Power Meter Integration**: Zigbee power meter connected to Home Assistant
- [ ] **API Endpoint**: `/api/power-status` endpoint created
- [ ] **Health Check**: `/health` endpoint created
- [ ] **Data Flow**: Power meter → Home Assistant → n8n → API endpoint

## 🚀 Deployment Steps

### Step 1: Download Plugin
```bash
# Download the plugin DLL
wget https://github.com/your-repo/releases/latest/download/Jellyfin.Plugin.WolWaker.dll

# Or copy from your development environment
cp /path/to/dev/Jellyfin.Plugin.WolWaker.dll ./Jellyfin.Plugin.WolWaker.dll
```

### Step 2: Install Plugin
```bash
# Create plugin directory
sudo mkdir -p /mnt/pool0/apps/jellyfin/config/plugins/WoLWaker

# Copy plugin DLL
sudo cp Jellyfin.Plugin.WolWaker.dll /mnt/pool0/apps/jellyfin/config/plugins/WoLWaker/

# Set correct permissions
sudo chown -R jellyfin:jellyfin /mnt/pool0/apps/jellyfin/config/plugins/WoLWaker
sudo chmod 644 /mnt/pool0/apps/jellyfin/config/plugins/WoLWaker/Jellyfin.Plugin.WolWaker.dll
```

### Step 3: Restart Jellyfin
```bash
# Restart Jellyfin service
sudo systemctl restart jellyfin

# Or if using Docker/TrueNAS Apps
# Restart the Jellyfin app from TrueNAS web interface
```

### Step 4: Verify Installation
1. **Check Jellyfin Logs**:
   ```bash
   sudo journalctl -u jellyfin -f
   # Look for: "WoL Waker plugin initialized successfully!"
   ```

2. **Check Plugin Status**:
   - Go to Jellyfin Admin → Plugins
   - Look for "WoL Waker" in the list
   - Ensure it shows as "Enabled"

3. **Test Basic Functionality**:
   ```bash
   # Test plugin health
   curl http://your-jellyfin-ip:8096/wol/health
   
   # Test WoL service
   curl http://your-jellyfin-ip:8096/wol/status
   ```

## ⚙️ Configuration

### Step 1: Access Configuration
1. Go to Jellyfin Admin → Plugins
2. Click on "WoL Waker"
3. Click "Configure"

### Step 2: Update Network Settings
```yaml
# Essential Settings
MAC Address: [YOUR-XEON-MAC]
Server IP: [YOUR-XEON-IP]
Server Port: [YOUR-SERVER-PORT]

# Power Monitoring (if enabled)
Power Monitoring Enabled: true
Power Monitor API URL: http://[YOUR-N8N-IP]:5678/api/power-status
Power Monitor Poll Interval: 5

# Fallback Monitoring
Use Fallback Monitoring: true
Fallback Check Interval: 15

# Timing
Wake Timeout: 300
Check Interval: 10
```

### Step 3: Test Configuration
```bash
# Test configuration endpoint
curl http://your-jellyfin-ip:8096/wol/config | jq '.'

# Test power monitoring (if enabled)
curl http://your-jellyfin-ip:8096/wol/power-test | jq '.'

# Test WoL functionality
curl http://your-jellyfin-ip:8096/wol/test | jq '.'
```

## 🔌 n8n Workflow Setup

### Required Endpoints

#### 1. Power Status Endpoint (`/api/power-status`)
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

#### 2. Health Check Endpoint (`/health`)
```json
{
  "status": "healthy",
  "service": "power-monitor",
  "timestamp": "2025-08-24T08:00:00Z"
}
```

### Workflow Structure
```
Zigbee Power Meter → Home Assistant → n8n → HTTP Response
     ↓
[Power Reading] → [Process Data] → [Return JSON]
```

### Example n8n Node Configuration
```javascript
// HTTP Response Node
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

## 🧪 Testing in Production

### Test 1: Basic Functionality
```bash
# Test plugin health
curl http://your-jellyfin-ip:8096/wol/health

# Expected response:
{
  "status": "healthy",
  "plugin": "WoL Waker",
  "version": "0.1.0",
  "timestamp": "2025-08-24T08:00:00Z"
}
```

### Test 2: WoL Functionality
```bash
# Test WoL service
curl http://your-jellyfin-ip:8096/wol/test

# Expected response:
{
  "success": true,
  "mac": "XX:XX:XX:XX:XX:XX",
  "timestamp": "2025-08-24T08:00:00Z"
}
```

### Test 3: Power Monitoring (if enabled)
```bash
# Test power monitoring
curl http://your-jellyfin-ip:8096/wol/power-test

# Expected response:
{
  "powerMonitorAvailable": true,
  "powerStatus": {
    "State": "Running",
    "PowerConsumption": 175.5,
    "Timestamp": "2025-08-24T08:00:00Z"
  }
}
```

### Test 4: Playback Simulation
```bash
# Simulate archival media request
curl -X POST http://your-jellyfin-ip:8096/wol/playback \
  -H "Content-Type: application/json" \
  -d '{
    "mediaPath": "/mnt/archival/movies/test.mkv",
    "mediaType": "movie",
    "userId": "test-user"
  }'

# Expected response:
{
  "CanPlayImmediately": false,
  "Message": "Server wake-up initiated, please wait...",
  "RequiresUserAction": true,
  "SessionId": "uuid-here",
  "EstimatedTimeToCompletion": 300
}
```

## 🔧 Troubleshooting

### Common Issues

#### Plugin Not Loading
```bash
# Check Jellyfin logs
sudo journalctl -u jellyfin -f

# Look for errors like:
# - "Failed to load plugin"
# - "Plugin initialization failed"
# - ".NET runtime not found"
```

#### WoL Not Working
```bash
# Test WoL manually
wakeonlan XX:XX:XX:XX:XX:XX

# Check network connectivity
ping [YOUR-XEON-IP]

# Verify MAC address format
# Should be: XX:XX:XX:XX:XX:XX
```

#### Power Monitoring Fails
```bash
# Test n8n endpoint directly
curl http://[YOUR-N8N-IP]:5678/api/power-status

# Check network connectivity
ping [YOUR-N8N-IP]

# Verify n8n workflow is running
# Check n8n logs for errors
```

#### Server Detection Issues
```bash
# Test server reachability
ping [YOUR-XEON-IP]

# Test port connectivity
telnet [YOUR-XEON-IP] [YOUR-SERVER-PORT]

# Check firewall settings
# Ensure ports are open
```

### Debug Mode
Enable detailed logging in the plugin configuration:
```yaml
Enable Detailed Logging: true
Log Level: Debug
```

Then check Jellyfin logs for detailed information:
```bash
sudo journalctl -u jellyfin -f | grep -i "wol\|power\|server"
```

## 📊 Monitoring & Maintenance

### Regular Checks
- [ ] **Weekly**: Check Jellyfin logs for errors
- [ ] **Monthly**: Test WoL functionality manually
- [ ] **Quarterly**: Verify power monitoring is working
- [ ] **Annually**: Review and update configuration

### Log Rotation
Ensure Jellyfin logs don't fill up your storage:
```bash
# Check log directory size
du -sh /var/log/jellyfin/

# Configure log rotation if needed
sudo nano /etc/logrotate.d/jellyfin
```

### Performance Monitoring
Monitor plugin performance:
```bash
# Check plugin response times
curl -w "@curl-format.txt" -o /dev/null -s "http://your-jellyfin-ip:8096/wol/health"

# Monitor resource usage
top -p $(pgrep jellyfin)
```

## 🆘 Support & Recovery

### Emergency Recovery
If the plugin causes issues:

1. **Disable Plugin**:
   - Go to Jellyfin Admin → Plugins
   - Disable "WoL Waker"
   - Restart Jellyfin

2. **Remove Plugin**:
   ```bash
   sudo rm /mnt/pool0/apps/jellyfin/config/plugins/WoLWaker/Jellyfin.Plugin.WolWaker.dll
   sudo systemctl restart jellyfin
   ```

3. **Restore from Backup**:
   ```bash
   sudo cp /backup/jellyfin-config/plugins/WoLWaker/Jellyfin.Plugin.WolWaker.dll /mnt/pool0/apps/jellyfin/config/plugins/WoLWaker/
   sudo systemctl restart jellyfin
   ```

### Getting Help
1. **Check Logs**: Enable detailed logging and check Jellyfin logs
2. **Test Components**: Test WoL, power monitoring, and network separately
3. **Verify Configuration**: Double-check all configuration values
4. **Check Network**: Ensure all components can reach each other

## 🎉 Success Criteria

Your deployment is successful when:

- [ ] **Plugin loads** without errors in Jellyfin logs
- [ ] **WoL works** - server wakes up when tested
- [ ] **Power monitoring** responds correctly (if enabled)
- [ ] **Fallback monitoring** works when power monitoring is disabled
- [ ] **Playback requests** trigger server wake-up automatically
- [ ] **User messages** display during wake-up process
- [ ] **Timeout handling** works correctly after 5 minutes

## 📚 Next Steps

After successful deployment:

1. **Test with Real Media**: Request actual archival media from Jellyfin
2. **Monitor Performance**: Watch for any performance issues
3. **Fine-tune Settings**: Adjust timeouts and intervals based on your server
4. **Document Configuration**: Save your working configuration for future reference
5. **Set Up Monitoring**: Consider monitoring the plugin's health in your infrastructure

**Congratulations! You now have a fully automated Wake-on-LAN system for your Jellyfin media server!** 🎉
