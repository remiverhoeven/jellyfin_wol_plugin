#!/usr/bin/env python3
"""
Power Monitoring Simulator for Jellyfin Wake-on-LAN Plugin Development

This simulator provides realistic power consumption data to test the plugin's
power monitoring integration without requiring actual hardware.

Simulates:
- Xeon server power states (off, waking, running, idle)
- Power consumption patterns during wake-up sequence
- Realistic timing and power curves
"""

import time
import random
import json
from datetime import datetime, timedelta
from flask import Flask, jsonify, request
from threading import Thread, Event
import logging

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = Flask(__name__)

class PowerState:
    """Represents different power states of the Xeon server"""
    OFF = "off"
    WAKING = "waking"
    RUNNING = "running"
    IDLE = "idle"
    SHUTTING_DOWN = "shutting_down"

class PowerSimulator:
    def __init__(self):
        self.current_state = PowerState.OFF
        self.current_wattage = 5.0  # Baseline power when off
        self.state_start_time = datetime.now()
        self.wake_progress = 0.0  # 0.0 to 1.0
        self.simulation_running = True
        self.state_lock = Event()
        
        # Power consumption characteristics (in watts)
        self.power_levels = {
            PowerState.OFF: 5.0,           # Idle power consumption
            PowerState.WAKING: 45.0,       # Average during wake-up
            PowerState.RUNNING: 180.0,     # Full operational power
            PowerState.IDLE: 120.0,        # Idle but running
            PowerState.SHUTTING_DOWN: 25.0 # Powering down
        }
        
        # Wake-up sequence timing (in seconds)
        self.wake_sequence = {
            "bios_post": 30,      # BIOS POST and initialization
            "os_boot": 60,        # Operating system boot
            "services": 45,       # Service startup
            "network": 15,        # Network initialization
            "total": 150          # Total wake-up time
        }
        
        # Start simulation thread
        self.simulation_thread = Thread(target=self._simulation_loop, daemon=True)
        self.simulation_thread.start()
    
    def _simulation_loop(self):
        """Main simulation loop that updates power consumption"""
        while self.simulation_running:
            try:
                if self.current_state == PowerState.WAKING:
                    self._update_wake_progress()
                elif self.current_state == PowerState.RUNNING:
                    self._simulate_running_variations()
                elif self.current_state == PowerState.IDLE:
                    self._simulate_idle_variations()
                
                time.sleep(1)  # Update every second
                
            except Exception as e:
                logger.error(f"Error in simulation loop: {e}")
                time.sleep(5)
    
    def _update_wake_progress(self):
        """Update wake progress and power consumption during wake-up"""
        elapsed = (datetime.now() - self.state_start_time).total_seconds()
        total_wake_time = self.wake_sequence["total"]
        
        if elapsed >= total_wake_time:
            # Wake-up complete
            self.current_state = PowerState.RUNNING
            self.current_wattage = self.power_levels[PowerState.RUNNING]
            self.state_start_time = datetime.now()
            self.wake_progress = 1.0
            logger.info("Wake-up sequence completed - server now running")
        else:
            # Update wake progress
            self.wake_progress = min(elapsed / total_wake_time, 1.0)
            
            # Simulate power consumption during wake-up
            # Power consumption increases as components power on
            base_power = self.power_levels[PowerState.OFF]
            target_power = self.power_levels[PowerState.RUNNING]
            
            # Use a curve that starts slow, accelerates, then levels off
            progress_factor = self.wake_progress ** 1.5
            self.current_wattage = base_power + (target_power - base_power) * progress_factor
            
            # Add some realistic variation
            variation = random.uniform(-5, 5)
            self.current_wattage = max(0, self.current_wattage + variation)
    
    def _simulate_running_variations(self):
        """Simulate power variations during normal operation"""
        # Simulate small power variations during operation
        variation = random.uniform(-10, 10)
        self.current_wattage = max(0, self.power_levels[PowerState.RUNNING] + variation)
        
        # Occasionally transition to idle state
        if random.random() < 0.001:  # 0.1% chance per second
            self.current_state = PowerState.IDLE
            self.current_wattage = self.power_levels[PowerState.IDLE]
            self.state_start_time = datetime.now()
            logger.info("Server transitioned to idle state")
    
    def _simulate_idle_variations(self):
        """Simulate power variations during idle state"""
        # Simulate small power variations during idle
        variation = random.uniform(-5, 5)
        self.current_wattage = max(0, self.power_levels[PowerState.IDLE] + variation)
        
        # Occasionally transition back to running state
        if random.random() < 0.002:  # 0.2% chance per second
            self.current_state = PowerState.RUNNING
            self.current_wattage = self.power_levels[PowerState.RUNNING]
            self.state_start_time = datetime.now()
            logger.info("Server transitioned to running state")
    
    def trigger_wake(self):
        """Trigger a wake-up sequence"""
        if self.current_state == PowerState.OFF:
            self.current_state = PowerState.WAKING
            self.state_start_time = datetime.now()
            self.wake_progress = 0.0
            self.current_wattage = self.power_levels[PowerState.WAKING]
            logger.info("Wake-up sequence triggered")
            return True
        else:
            logger.info(f"Cannot trigger wake - server is already {self.current_state}")
            return False
    
    def trigger_shutdown(self):
        """Trigger a shutdown sequence"""
        if self.current_state in [PowerState.RUNNING, PowerState.IDLE]:
            self.current_state = PowerState.SHUTTING_DOWN
            self.state_start_time = datetime.now()
            self.current_wattage = self.power_levels[PowerState.SHUTTING_DOWN]
            logger.info("Shutdown sequence triggered")
            
            # Schedule transition to off state
            def shutdown_complete():
                time.sleep(10)  # 10 second shutdown
                self.current_state = PowerState.OFF
                self.current_wattage = self.power_levels[PowerState.OFF]
                logger.info("Shutdown complete - server is now off")
            
            Thread(target=shutdown_complete, daemon=True).start()
            return True
        else:
            logger.info(f"Cannot trigger shutdown - server is {self.current_state}")
            return False
    
    def get_power_status(self):
        """Get current power status"""
        return {
            "wattage": round(self.current_wattage, 2),
            "baselineWattage": self.power_levels[PowerState.OFF],
            "fullPowerWattage": self.power_levels[PowerState.RUNNING],
            "state": self.current_state,
            "wakeProgress": round(self.wake_progress, 3),
            "timestamp": datetime.now().isoformat(),
            "status": "monitoring"
        }
    
    def get_server_status(self):
        """Get comprehensive server status"""
        return {
            "power": self.get_power_status(),
            "network": {
                "reachable": self.current_state not in [PowerState.OFF, PowerState.WAKING],
                "ip": "172.20.0.3",  # Mock Xeon IP in our network
                "response_time": random.uniform(1, 5) if self.current_state not in [PowerState.OFF, PowerState.WAKING] else None
            },
            "services": {
                "jellyfin": self.current_state in [PowerState.RUNNING, PowerState.IDLE],
                "http": self.current_state in [PowerState.RUNNING, PowerState.IDLE],
                "ssh": self.current_state in [PowerState.RUNNING, PowerState.IDLE]
            },
            "uptime": (datetime.now() - self.state_start_time).total_seconds() if self.current_state != PowerState.OFF else 0
        }

# Global simulator instance
simulator = PowerSimulator()

@app.route('/api/power-status/current', methods=['GET'])
def get_power_status():
    """Get current power consumption status"""
    return jsonify(simulator.get_power_status())

@app.route('/api/power-status/server', methods=['GET'])
def get_server_status():
    """Get comprehensive server status"""
    return jsonify(simulator.get_server_status())

@app.route('/api/power-status/wake', methods=['POST'])
def trigger_wake():
    """Trigger a wake-up sequence"""
    success = simulator.trigger_wake()
    return jsonify({
        "success": success,
        "message": "Wake-up triggered" if success else "Cannot trigger wake-up",
        "timestamp": datetime.now().isoformat()
    })

@app.route('/api/power-status/shutdown', methods=['POST'])
def trigger_shutdown():
    """Trigger a shutdown sequence"""
    success = simulator.trigger_shutdown()
    return jsonify({
        "success": success,
        "message": "Shutdown triggered" if success else "Cannot trigger shutdown",
        "timestamp": datetime.now().isoformat()
    })

@app.route('/api/power-status/state', methods=['GET'])
def get_current_state():
    """Get current server state"""
    return jsonify({
        "state": simulator.current_state,
        "wattage": simulator.current_wattage,
        "timestamp": datetime.now().isoformat()
    })

@app.route('/health', methods=['GET'])
def health_check():
    """Health check endpoint"""
    return jsonify({
        "status": "healthy",
        "service": "power-monitor-simulator",
        "timestamp": datetime.now().isoformat()
    })

@app.route('/', methods=['GET'])
def index():
    """Main page with API documentation"""
    return """
    <html>
    <head><title>Power Monitor Simulator</title></head>
    <body>
        <h1>Power Monitor Simulator</h1>
        <p>This simulator provides realistic power consumption data for testing the Jellyfin Wake-on-LAN Plugin.</p>
        
        <h2>Available Endpoints:</h2>
        <ul>
            <li><strong>GET /api/power-status/current</strong> - Current power consumption</li>
            <li><strong>GET /api/power-status/server</strong> - Comprehensive server status</li>
            <li><strong>POST /api/power-status/wake</strong> - Trigger wake-up sequence</li>
            <li><strong>POST /api/power-status/shutdown</strong> - Trigger shutdown sequence</li>
            <li><strong>GET /api/power-status/state</strong> - Current server state</li>
            <li><strong>GET /health</strong> - Health check</li>
        </ul>
        
        <h2>Current Status:</h2>
        <div id="status">Loading...</div>
        
        <h2>Controls:</h2>
        <button onclick="triggerWake()">Trigger Wake</button>
        <button onclick="triggerShutdown()">Trigger Shutdown</button>
        
        <script>
            function updateStatus() {
                fetch('/api/power-status/current')
                    .then(response => response.json())
                    .then(data => {
                        document.getElementById('status').innerHTML = 
                            '<pre>' + JSON.stringify(data, null, 2) + '</pre>';
                    });
            }
            
            function triggerWake() {
                fetch('/api/power-status/wake', {method: 'POST'})
                    .then(response => response.json())
                    .then(data => alert(data.message));
            }
            
            function triggerShutdown() {
                fetch('/api/power-status/shutdown', {method: 'POST'})
                    .then(response => response.json())
                    .then(data => alert(data.message));
            }
            
            // Update status every 2 seconds
            setInterval(updateStatus, 2000);
            updateStatus();
        </script>
    </body>
    </html>
    """

if __name__ == '__main__':
    logger.info("Starting Power Monitor Simulator...")
    logger.info("Available endpoints:")
    logger.info("  - GET  /api/power-status/current")
    logger.info("  - GET  /api/power-status/server")
    logger.info("  - POST /api/power-status/wake")
    logger.info("  - POST /api/power-status/shutdown")
    logger.info("  - GET  /health")
    
    app.run(host='0.0.0.0', port=5000, debug=False)
