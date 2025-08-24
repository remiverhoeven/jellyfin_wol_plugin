# 🔧 WoL Waker Plugin - Configuration Template

## 📝 Copy and Modify This Template

Replace the placeholder values with your actual network configuration:

### 🌐 Network Configuration

```yaml
# Your Xeon server details
MAC Address: 00:11:22:33:44:55          # ← REPLACE: Your Xeon server's MAC address
Server IP: 192.168.2.17                  # ← REPLACE: Your Xeon server's IP address
Server Port: 80                          # ← REPLACE: Port to check (80 for HTTP, 22 for SSH, etc.)

# Network broadcast settings (usually don't need to change)
Broadcast Address: 255.255.255.255      # ← REPLACE: Your network broadcast address if different
Broadcast Port: 9                        # ← REPLACE: WoL port (usually 9)
```

### ⚡ Power Monitoring Configuration (Optional)

```yaml
# Enable/disable power monitoring
Power Monitoring Enabled: true           # ← REPLACE: true if you have n8n + power meter, false if not

# n8n API endpoint (only needed if Power Monitoring Enabled = true)
Power Monitor API URL: http://192.168.1.100:5678/api/power-status
# ↑ REPLACE: http://[your-n8n-ip]:[your-n8n-port]/[your-endpoint]

# API authentication (if required)
Power Monitor API Key:                  # ← REPLACE: Your API key, or leave empty if no auth

# How often to check power status
Power Monitor Poll Interval: 5           # ← REPLACE: Seconds between power checks (2-10 recommended)
```

### ⏱️ Timing Configuration

```yaml
# How long to wait for server to wake up
Wake Timeout: 300                        # ← REPLACE: Maximum wait time in seconds (300 = 5 minutes)

# How often to check server status
Check Interval: 10                       # ← REPLACE: Seconds between status checks (5-15 recommended)

# Safety delays
Cooldown Seconds: 300                    # ← REPLACE: Wait time between wake attempts (300 = 5 minutes)
Min Wake Interval: 60                    # ← REPLACE: Minimum time between attempts (60 = 1 minute)
Max Wake Attempts: 3                     # ← REPLACE: Stop after this many failed attempts
```

### 🔄 Fallback Monitoring

```yaml
# Use network checks if power monitoring fails
Use Fallback Monitoring: true            # ← REPLACE: true for better reliability, false for power-only

# How often to perform fallback checks
Fallback Check Interval: 15              # ← REPLACE: Seconds between fallback checks (10-30 recommended)
```

### 🎯 Behavior Settings

```yaml
# Core functionality
Enable Auto Wake: true                   # ← REPLACE: true to send WoL packets, false for monitoring only
Wake On Playback Start: true            # ← REPLACE: true to wake server when media is requested
Show User Messages: true                 # ← REPLACE: true to show progress messages to users

# Logging
Enable Detailed Logging: true            # ← REPLACE: true for troubleshooting, false for production
Log Level: Information                   # ← REPLACE: Debug, Information, Warning, or Error
```

## 🚀 Quick Start Configuration

### For Users WITH Power Monitoring (n8n + Zigbee power meter):

```yaml
MAC Address: [YOUR-XEON-MAC]
Server IP: [YOUR-XEON-IP]
Power Monitoring Enabled: true
Power Monitor API URL: http://[YOUR-N8N-IP]:5678/api/power-status
Wake Timeout: 300
Use Fallback Monitoring: true
```

### For Users WITHOUT Power Monitoring (network-only):

```yaml
MAC Address: [YOUR-XEON-MAC]
Server IP: [YOUR-XEON-IP]
Power Monitoring Enabled: false
Use Fallback Monitoring: true
Fallback Check Interval: 10
Wake Timeout: 300
```

## 🔍 How to Find Your Values

### MAC Address
```bash
# On your Xeon server
ip link show

# Or check your router's DHCP client list
# Look for your server's hostname or IP
```

### Server IP Address
```bash
# On your Xeon server
ip addr show

# Or check your router's DHCP client list
# Usually starts with 192.168.x.x or 10.0.x.x
```

### n8n IP Address
```bash
# On your n8n server
ip addr show

# Or check your router's DHCP client list
# Look for the device running n8n
```

### Network Broadcast Address
```bash
# Usually 255.255.255.255 for local networks
# If you're unsure, try this first
```

## ✅ Configuration Checklist

Before testing, ensure you have:

- [ ] **MAC Address**: Correct Xeon server MAC address
- [ ] **Server IP**: Correct Xeon server IP address
- [ ] **Server Port**: Port that's actually open on your Xeon server
- [ ] **n8n URL**: Correct n8n API endpoint (if using power monitoring)
- [ ] **Network Access**: Jellyfin can reach your Xeon server and n8n instance
- [ ] **WoL Enabled**: Wake-on-LAN is enabled in your Xeon server's BIOS

## 🧪 Test Your Configuration

After updating the configuration:

1. **Save** the configuration in Jellyfin Admin → Plugins → WoL Waker → Configure
2. **Test basic functionality**: `curl http://your-jellyfin:8096/wol/health`
3. **Test WoL**: `curl http://your-jellyfin:8096/wol/test`
4. **Test power monitoring** (if enabled): `curl http://your-jellyfin:8096/wol/power-test`

## 🆘 Need Help?

If you're unsure about any values:

1. **Start with the defaults** - they're usually correct
2. **Test incrementally** - get basic WoL working before adding power monitoring
3. **Check the logs** - enable detailed logging to see what's happening
4. **Use fallback monitoring** - it's more reliable than power monitoring alone

## 📱 Example: My Home Network

Here's what my configuration looks like:

```yaml
MAC Address: AA:BB:CC:DD:EE:FF          # My Xeon server's MAC
Server IP: 192.168.1.50                  # My Xeon server's IP
Server Port: 80                          # HTTP port
Power Monitoring Enabled: true            # I have n8n + Zigbee power meter
Power Monitor API URL: http://192.168.1.100:5678/api/power-status
Power Monitor Poll Interval: 5           # Check every 5 seconds
Wake Timeout: 300                        # Wait up to 5 minutes
Use Fallback Monitoring: true            # Backup to network checks
Fallback Check Interval: 15              # Check every 15 seconds
```

**Copy this template, replace the values with your network details, and you're ready to go!** 🎉
