# Docker Development Environment

## Overview

This Docker-based development environment creates a local setup that closely mirrors your TrueNAS Scale production environment. It includes all the services you'll need to develop and test the Jellyfin Wake-on-LAN Plugin.

## 🏗️ **Architecture Comparison**

### **Production Environment (TrueNAS Scale)**
```
N100 Machine (192.168.2.226)          Xeon Server (192.168.2.17)
├── Jellyfin Server                    ├── Archival Media Storage
├── n8n Workflows                      ├── Power Monitoring
├── Local Media Database               └── Wake-on-LAN Target
└── Plugin Development
```

### **Development Environment (Docker)**
```
Local Machine (localhost)
├── Jellyfin Server (port 8096)       Mock Xeon Server (port 8080)
├── n8n (port 5678)                   ├── Simulated Storage
├── Power Monitor Simulator (port 5000)├── HTTP Service
└── Plugin Development                 └── Network Service
```

## 🐳 **Services Included**

### **1. Jellyfin Server (Port 8096)**
- **Purpose**: Main media server for testing
- **Similar to**: Your N100 Jellyfin instance
- **Features**: 
  - Plugin support
  - Media library management
  - User interface testing

### **2. n8n (Port 5678)**
- **Purpose**: Workflow automation platform
- **Similar to**: Your N100 n8n instance
- **Features**:
  - Power monitoring workflows
  - API endpoint testing
  - Integration testing

### **3. Power Monitor Simulator (Port 5000)**
- **Purpose**: Simulates your Zigbee power monitor
- **Similar to**: Real power monitoring hardware
- **Features**:
  - Realistic power consumption patterns
  - Wake-up sequence simulation
  - API endpoints matching production

### **4. Mock Xeon Server (Port 8080)**
- **Purpose**: Simulates your Xeon server
- **Similar to**: Your actual Xeon server
- **Features**:
  - HTTP service simulation
  - Status monitoring
  - Network connectivity testing

## 🚀 **Quick Start**

### **Prerequisites**
- Docker Desktop installed and running
- .NET 8.0 SDK installed
- Git repository cloned

### **Setup Commands**
```bash
# Navigate to plugin directory
cd jellyfin_wol_plugin

# Run the automated setup
./setup_docker_dev.sh

# Or manually start services
docker-compose up -d
```

### **What the Setup Script Does**
1. ✅ **Checks prerequisites** (Docker, .NET 8.0)
2. ✅ **Creates directory structure** for all services
3. ✅ **Sets up test media** (local and archival)
4. ✅ **Starts Docker services** (Jellyfin, n8n, power monitor, mock Xeon)
5. ✅ **Builds and installs plugin** to Jellyfin
6. ✅ **Creates test configuration** for the plugin
7. ✅ **Tests all services** for accessibility

## 📁 **Directory Structure**

```
jellyfin_wol_plugin/
├── docker-compose.yml              # Docker services configuration
├── setup_docker_dev.sh            # Automated setup script
├── Jellyfin.Plugin.WolWaker/      # Plugin source code
├── jellyfin-config/               # Jellyfin configuration (mounted to container)
│   └── plugins/WoLWaker/         # Plugin installation directory
├── n8n-data/                      # n8n data persistence
├── n8n-workflows/                 # n8n workflow definitions
├── test-media/                    # Local media (simulates N100 storage)
├── test-media-archival/           # Archival media (simulates Xeon storage)
├── mock-xeon/                     # Mock Xeon server files
└── power-monitor-sim/             # Power monitoring simulator
    └── power_simulator.py         # Python power simulation service
```

## 🔧 **Service Configuration**

### **Jellyfin Configuration**
- **Port**: 8096
- **Config Directory**: `./jellyfin-config`
- **Media Directories**: 
  - Local: `./test-media`
  - Archival: `./test-media-archival`
- **Plugin Support**: Enabled

### **n8n Configuration**
- **Port**: 5678
- **Data Directory**: `./n8n-data`
- **Workflows Directory**: `./n8n-workflows`
- **Authentication**: Disabled for development

### **Power Monitor Simulator**
- **Port**: 5000
- **API Endpoints**:
  - `GET /api/power-status/current` - Current power consumption
  - `GET /api/power-status/server` - Server status
  - `POST /api/power-status/wake` - Trigger wake-up
  - `POST /api/power-status/shutdown` - Trigger shutdown

### **Mock Xeon Server**
- **Port**: 8080
- **Purpose**: Simulates Xeon server responses
- **Features**: Status monitoring, service simulation

## 🧪 **Testing Scenarios**

### **1. Basic Plugin Functionality**
```bash
# Test plugin loading
curl http://localhost:8096/System/Info

# Check plugin appears in Jellyfin dashboard
# Navigate to: http://localhost:8096/Dashboard/Plugins
```

### **2. Power Monitoring Integration**
```bash
# Test power status API
curl http://localhost:5000/api/power-status/current

# Trigger wake-up sequence
curl -X POST http://localhost:5000/api/power-status/wake

# Monitor power consumption changes
curl http://localhost:5000/api/power-status/server
```

### **3. Media Interception Testing**
1. **Add media libraries** in Jellyfin pointing to test directories
2. **Try to play** media from archival storage
3. **Observe plugin behavior** and status messages
4. **Test wake-up sequence** with power monitoring

### **4. Network Connectivity Testing**
```bash
# Test mock Xeon server
curl http://localhost:8080

# Test network reachability
ping 172.20.0.3  # Mock Xeon IP in Docker network
```

## 🔄 **Development Workflow**

### **Daily Development Cycle**
1. **Make changes** to plugin code
2. **Build plugin**: `dotnet build -c Debug`
3. **Copy to Jellyfin**: Plugin auto-installs during setup
4. **Test in Jellyfin**: Access http://localhost:8096
5. **Debug and iterate**: Use Jellyfin logs and plugin logs
6. **Commit changes**: Git workflow

### **Plugin Updates**
```bash
# Build updated plugin
cd Jellyfin.Plugin.WolWaker
dotnet build -c Debug

# Copy to Jellyfin (plugin directory is mounted)
cp bin/Debug/net8.0/Jellyfin.Plugin.WolWaker.dll ../jellyfin-config/plugins/WoLWaker/

# Restart Jellyfin to reload plugin
docker-compose restart jellyfin-dev
```

## 🐛 **Troubleshooting**

### **Common Issues**

#### **Services Not Starting**
```bash
# Check Docker status
docker-compose ps

# View service logs
docker-compose logs jellyfin-dev
docker-compose logs n8n-dev
docker-compose logs power-monitor-sim

# Restart services
docker-compose restart
```

#### **Plugin Not Loading**
1. **Check Jellyfin logs**: Dashboard → Advanced → Logs
2. **Verify plugin directory**: `./jellyfin-config/plugins/WoLWaker/`
3. **Check file permissions**: Plugin DLL should be readable
4. **Restart Jellyfin**: `docker-compose restart jellyfin-dev`

#### **Power Monitoring Issues**
1. **Test API endpoint**: `curl http://localhost:5000/health`
2. **Check simulator logs**: `docker-compose logs power-monitor-sim`
3. **Verify network connectivity**: All services should be on same Docker network

### **Debug Mode**
```bash
# Enable debug logging in Jellyfin
# Dashboard → Advanced → Logging → Set Log Level to Debug

# View real-time logs
docker-compose logs -f jellyfin-dev
```

## 🚀 **Production Deployment**

### **From Development to Production**
1. **Test locally** with Docker environment
2. **Build release version**: `dotnet build -c Release`
3. **Deploy to TrueNAS**: Copy plugin to production Jellyfin
4. **Update configuration**: Use production network settings
5. **Test in production**: Verify with real hardware

### **Configuration Differences**
```json
// Development (Docker)
{
  "ServerIp": "172.20.0.3",
  "PowerMonitorApiUrl": "http://localhost:5000/api/power-status"
}

// Production (TrueNAS)
{
  "ServerIp": "192.168.2.17",
  "PowerMonitorApiUrl": "http://192.168.2.226:5678/api/power-status"
}
```

## 📚 **Additional Resources**

### **Documentation**
- [DEVELOPMENT.md](DEVELOPMENT.md) - General development guide
- [README.md](README.md) - Plugin overview and usage
- [jellyfin_wol_technical_spec.md](../jellyfin_wol_technical_spec.md) - Technical specification

### **Useful Commands**
```bash
# Start all services
docker-compose up -d

# Stop all services
docker-compose down

# View service status
docker-compose ps

# View service logs
docker-compose logs [service-name]

# Restart specific service
docker-compose restart [service-name]

# Rebuild and restart services
docker-compose up -d --build
```

### **Network Information**
- **Docker Network**: `jellyfin_wol_plugin_jellyfin-network`
- **Subnet**: `172.20.0.0/16`
- **Jellyfin IP**: `172.20.0.2`
- **n8n IP**: `172.20.0.4`
- **Mock Xeon IP**: `172.20.0.3`
- **Power Monitor IP**: `172.20.0.5`

---

**Environment**: Docker Development  
**Purpose**: TrueNAS Scale Development Mirror  
**Last Updated**: December 2024
