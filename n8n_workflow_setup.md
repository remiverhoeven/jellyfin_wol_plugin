# n8n Workflow Setup for Power Monitoring

## Overview

This document describes how to set up an n8n workflow to provide power monitoring data to the Jellyfin Wake-on-LAN Plugin. The workflow will expose a REST API endpoint that the plugin can poll to get real-time power consumption information.

## 🎯 **Expected API Endpoints**

### **1. Power Status Endpoint**
```
GET /api/power-status/current
```

**Response Format:**
```json
{
  "wattage": 45.2,
  "baselineWattage": 5.0,
  "fullPowerWattage": 180.0,
  "state": "waking",
  "wakeProgress": 0.3,
  "timestamp": "2024-12-19T10:30:00Z",
  "status": "monitoring"
}
```

### **2. Server Status Endpoint**
```
GET /api/power-status/server
```

**Response Format:**
```json
{
  "power": {
    "wattage": 45.2,
    "baselineWattage": 5.0,
    "fullPowerWattage": 180.0,
    "state": "waking",
    "wakeProgress": 0.3,
    "timestamp": "2024-12-19T10:30:00Z"
  },
  "network": {
    "reachable": false,
    "ip": "192.168.2.17",
    "response_time": null
  },
  "services": {
    "jellyfin": false,
    "http": false,
    "ssh": false
  },
  "uptime": 0
}
```

## 🔧 **n8n Workflow Setup**

### **Prerequisites**
- n8n running on your N100 machine (port 5678)
- Access to power monitoring data (Zigbee, Home Assistant, etc.)
- Network access to the Xeon server (192.168.2.17)

### **Workflow Structure**

#### **1. Power Data Collection Node**
- **Type**: HTTP Request, MQTT, or Home Assistant
- **Purpose**: Collect current power consumption data
- **Data Source**: Your power monitoring hardware
- **Frequency**: Every 5-10 seconds

#### **2. Data Processing Node**
- **Type**: Function or Code
- **Purpose**: Process raw power data and calculate wake progress
- **Logic**: 
  - Determine server state based on power consumption
  - Calculate wake progress percentage
  - Add timestamps and metadata

#### **3. API Response Node**
- **Type**: Respond to Webhook
- **Purpose**: Serve API requests from the plugin
- **Routes**: 
  - `/api/power-status/current`
  - `/api/power-status/server`

#### **4. Health Check Node**
- **Type**: Function
- **Purpose**: Monitor Xeon server health
- **Checks**:
  - Network connectivity (ping)
  - Service availability (HTTP, SSH)
  - Response times

## 📊 **Power State Detection Logic**

### **Power Consumption Thresholds**
```javascript
// Example JavaScript logic for n8n Function node
const currentWattage = $input.first().json.wattage;
const baselineWattage = 5.0;      // Server off/idle
const fullPowerWattage = 180.0;   // Server fully operational

let serverState = 'off';
let wakeProgress = 0.0;

if (currentWattage <= baselineWattage + 5) {
    serverState = 'off';
    wakeProgress = 0.0;
} else if (currentWattage >= fullPowerWattage - 20) {
    serverState = 'running';
    wakeProgress = 1.0;
} else {
    serverState = 'waking';
    // Calculate progress based on power consumption
    wakeProgress = Math.min(1.0, (currentWattage - baselineWattage) / (fullPowerWattage - baselineWattage));
}

return {
    wattage: currentWattage,
    baselineWattage: baselineWattage,
    fullPowerWattage: fullPowerWattage,
    state: serverState,
    wakeProgress: Math.round(wakeProgress * 1000) / 1000,
    timestamp: new Date().toISOString(),
    status: 'monitoring'
};
```

### **Wake Progress Calculation**
```javascript
// More sophisticated wake progress calculation
function calculateWakeProgress(wattage, baseline, fullPower) {
    if (wattage <= baseline + 5) return 0.0;
    if (wattage >= fullPower - 20) return 1.0;
    
    // Use a curve that accounts for power consumption patterns during boot
    const normalizedPower = (wattage - baseline) / (fullPower - baseline);
    
    // Power consumption typically follows a curve: slow start, rapid increase, then level off
    if (normalizedPower < 0.3) {
        // Early boot phase - slower progress
        return normalizedPower * 0.4;
    } else if (normalizedPower < 0.7) {
        // Main boot phase - faster progress
        return 0.12 + (normalizedPower - 0.3) * 1.45;
    } else {
        // Final phase - slower progress
        return 0.7 + (normalizedPower - 0.7) * 0.75;
    }
}
```

## 🌐 **Network Health Monitoring**

### **Ping Test**
```javascript
// Ping the Xeon server to check network connectivity
const pingResult = await $http.get({
    url: 'http://192.168.2.17:8080/health',
    timeout: 5000
});

const networkStatus = {
    reachable: pingResult.status === 200,
    ip: '192.168.2.17',
    response_time: pingResult.responseTime || null
};
```

### **Service Availability Check**
```javascript
// Check if Jellyfin and other services are available
const services = {};

try {
    const jellyfinResponse = await $http.get({
        url: 'http://192.168.2.17:8096/System/Info',
        timeout: 5000
    });
    services.jellyfin = jellyfinResponse.status === 200;
} catch (error) {
    services.jellyfin = false;
}

try {
    const httpResponse = await $http.get({
        url: 'http://192.168.2.17:80',
        timeout: 5000
    });
    services.http = httpResponse.status === 200;
} catch (error) {
    services.http = false;
}

try {
    const sshResponse = await $http.get({
        url: 'http://192.168.2.17:22',
        timeout: 5000
    });
    services.ssh = sshResponse.status === 200;
} catch (error) {
    services.ssh = false;
}
```

## 🚀 **Complete n8n Workflow Example**

### **Workflow JSON Export**
```json
{
  "name": "Power Monitoring for Jellyfin WoL Plugin",
  "nodes": [
    {
      "id": "power-data-collection",
      "name": "Collect Power Data",
      "type": "n8n-nodes-base.httpRequest",
      "parameters": {
        "url": "http://your-power-monitor/api/current",
        "method": "GET",
        "timeout": 5000
      }
    },
    {
      "id": "process-power-data",
      "name": "Process Power Data",
      "type": "n8n-nodes-base.function",
      "parameters": {
        "functionCode": "// Power processing logic here"
      }
    },
    {
      "id": "api-endpoint-current",
      "name": "Current Power Status API",
      "type": "n8n-nodes-base.respondToWebhook",
      "parameters": {
        "path": "api/power-status/current",
        "responseMode": "responseNode",
        "options": {}
      }
    },
    {
      "id": "api-endpoint-server",
      "name": "Server Status API",
      "type": "n8n-nodes-base.respondToWebhook",
      "parameters": {
        "path": "api/power-status/server",
        "responseMode": "responseNode",
        "options": {}
      }
    }
  ],
  "connections": {
    "power-data-collection": {
      "main": [["process-power-data"]]
    },
    "process-power-data": {
      "main": [["api-endpoint-current"], ["api-endpoint-server"]]
    }
  }
}
```

## 🔄 **Alternative: Home Assistant Integration**

If you prefer to use Home Assistant instead of n8n:

### **Home Assistant REST API**
```yaml
# configuration.yaml
rest:
  - resource: http://192.168.2.226:5678/api/power-status/current
    scan_interval: 5
    name: "Power Monitor API"
```

### **Home Assistant Automation**
```yaml
# automations.yaml
- alias: "Power Monitor API"
  trigger:
    platform: time_pattern
    seconds: "/5"
  action:
    - service: rest_command.call_power_api
    - service: input_text.set_value
      target:
        entity_id: input_text.power_status
      value: "{{ states('sensor.power_monitor') }}"
```

## 📋 **Testing the API**

### **Test Commands**
```bash
# Test power status endpoint
curl http://192.168.2.226:5678/api/power-status/current

# Test server status endpoint
curl http://192.168.2.226:5678/api/power-status/server

# Test from plugin (when running)
curl http://localhost:8096/wol/status
```

### **Expected Test Results**
```json
{
  "power": {
    "wattage": 45.2,
    "state": "waking",
    "wakeProgress": 0.3
  },
  "network": {
    "reachable": false
  },
  "services": {
    "jellyfin": false
  }
}
```

## 🎯 **Integration with Plugin**

### **Plugin Configuration**
```json
{
  "PowerMonitoringEnabled": true,
  "PowerMonitorApiUrl": "http://192.168.2.226:5678/api/power-status",
  "PowerMonitorPollInterval": 5
}
```

### **Plugin Usage**
1. **Plugin polls** the n8n API every 5 seconds
2. **Power data** determines wake progress
3. **User sees** real-time status updates
4. **Plugin can** estimate time remaining

## 🚨 **Troubleshooting**

### **Common Issues**
1. **API not accessible**: Check n8n firewall settings
2. **CORS errors**: Configure n8n CORS settings
3. **Data not updating**: Check workflow execution frequency
4. **Network timeouts**: Adjust timeout values in plugin

### **Debug Steps**
1. Test API endpoints manually
2. Check n8n execution logs
3. Verify network connectivity
4. Test with curl commands

---

**Environment**: n8n on N100 (192.168.2.226:5678)  
**Target**: Xeon Server (192.168.2.17)  
**Purpose**: Power monitoring for Jellyfin WoL Plugin  
**Last Updated**: December 2024
