#!/usr/bin/env python3
"""
TrueNAS Startup Script - Wake Xeon Server Before Starting Apps

This script should be run as a TrueNAS init/task script to ensure
the Xeon server is awake before starting dependent applications.
"""

import socket
import time
import subprocess
import logging
import binascii
from datetime import datetime

# Configuration
XEON_MAC = "a4:ae:11:19:ac:33"
XEON_IP = "192.168.2.17"
BROADCAST_IP = "192.168.2.255"  # or "255.255.255.255"
WOL_PORT = 9
MAX_WAIT_TIME = 300  # 5 minutes
PING_TIMEOUT = 5
DEPENDENT_APPS = ["jellyfin", "radarr", "sonarr"]

# Setup logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('/var/log/xeon_wol_startup.log'),
        logging.StreamHandler()
    ]
)
logger = logging.getLogger(__name__)

def create_magic_packet(mac_address):
    """Create Wake-on-LAN magic packet."""
    # Remove separators and convert to uppercase
    clean_mac = mac_address.replace(":", "").replace("-", "").replace(".", "").upper()
    
    if len(clean_mac) != 12:
        raise ValueError(f"Invalid MAC address length: {len(clean_mac)}")
    
    # Convert to bytes
    mac_bytes = binascii.unhexlify(clean_mac)
    
    # Magic packet: 6 bytes of 0xFF + MAC address repeated 16 times
    packet = b'\xff' * 6 + mac_bytes * 16
    
    return packet

def send_wol_packet(mac_address, broadcast_ip=BROADCAST_IP, port=WOL_PORT):
    """Send Wake-on-LAN packet."""
    try:
        packet = create_magic_packet(mac_address)
        
        logger.info(f"Sending WoL packet to {mac_address} via {broadcast_ip}:{port}")
        
        with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as sock:
            sock.setsockopt(socket.SOL_SOCKET, socket.SO_BROADCAST, 1)
            result = sock.sendto(packet, (broadcast_ip, port))
            
        logger.info(f"WoL packet sent successfully ({result} bytes)")
        return True
        
    except Exception as e:
        logger.error(f"Failed to send WoL packet: {e}")
        return False

def ping_host(ip_address, timeout=PING_TIMEOUT):
    """Check if host is reachable via ping."""
    try:
        result = subprocess.run(
            ["ping", "-c", "1", "-W", str(timeout), ip_address],
            capture_output=True,
            text=True,
            timeout=timeout + 2
        )
        return result.returncode == 0
    except subprocess.TimeoutExpired:
        return False
    except Exception as e:
        logger.error(f"Ping error: {e}")
        return False

def wait_for_server(ip_address, max_wait=MAX_WAIT_TIME):
    """Wait for server to respond to ping."""
    logger.info(f"Waiting for {ip_address} to respond (max {max_wait}s)...")
    
    start_time = time.time()
    while time.time() - start_time < max_wait:
        if ping_host(ip_address):
            elapsed = int(time.time() - start_time)
            logger.info(f"Server {ip_address} is responding! (took {elapsed}s)")
            return True
        
        logger.info(f"Server not responding yet, waiting 10s...")
        time.sleep(10)
    
    logger.warning(f"Server {ip_address} did not respond within {max_wait}s")
    return False

def start_truenas_app(app_name):
    """Start a TrueNAS app using midclt."""
    try:
        logger.info(f"Starting TrueNAS app: {app_name}")
        
        result = subprocess.run(
            ["midclt", "call", "app.start", app_name],
            capture_output=True,
            text=True,
            timeout=60
        )
        
        if result.returncode == 0:
            logger.info(f"Successfully started {app_name}")
            return True
        else:
            logger.error(f"Failed to start {app_name}: {result.stderr}")
            return False
            
    except Exception as e:
        logger.error(f"Error starting {app_name}: {e}")
        return False

def main():
    """Main startup sequence."""
    logger.info("=== TrueNAS Xeon WoL Startup Script ===")
    logger.info(f"Target: {XEON_IP} (MAC: {XEON_MAC})")
    logger.info(f"Dependent apps: {', '.join(DEPENDENT_APPS)}")
    
    # Check if Xeon is already online
    if ping_host(XEON_IP):
        logger.info(f"Xeon server {XEON_IP} is already online!")
    else:
        logger.info(f"Xeon server {XEON_IP} is offline, sending WoL...")
        
        # Send WoL packet
        if not send_wol_packet(XEON_MAC):
            logger.error("Failed to send WoL packet, but continuing...")
        
        # Wait for server to respond
        if not wait_for_server(XEON_IP):
            logger.warning("Server did not respond, but continuing with app startup...")
    
    # Small delay to ensure services are stable
    logger.info("Waiting 30s for server to stabilize...")
    time.sleep(30)
    
    # Start dependent apps
    for app in DEPENDENT_APPS:
        try:
            start_truenas_app(app)
            time.sleep(10)  # Delay between app starts
        except Exception as e:
            logger.error(f"Failed to start {app}: {e}")
    
    logger.info("=== Startup sequence completed ===")

if __name__ == "__main__":
    main()
