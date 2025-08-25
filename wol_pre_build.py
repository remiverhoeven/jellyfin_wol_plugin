#!/usr/bin/env python3
"""
Docker Pre-Build WoL Script
Wakes Xeon server before building containers or starting TrueNAS apps
"""

import os
import socket
import time
import binascii
import logging
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

# Configuration from environment (or defaults)
XEON_MAC = os.getenv('XEON_MAC', 'a4:ae:11:19:ac:33')
XEON_IP = os.getenv('XEON_IP', '192.168.2.17')
BROADCAST_IP = os.getenv('BROADCAST_IP', '192.168.2.255')
WOL_PORT = int(os.getenv('WOL_PORT', '9'))
MAX_WAIT_TIME = int(os.getenv('MAX_WAIT_TIME', '300'))
PING_TIMEOUT = int(os.getenv('PING_TIMEOUT', '5'))

# Setup logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
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
        import subprocess
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
            logger.info(f"✅ Server {ip_address} is responding! (took {elapsed}s)")
            return True
        
        logger.info(f"⏳ Server not responding yet, waiting 10s...")
        time.sleep(10)
    
    logger.warning(f"⚠️ Server {ip_address} did not respond within {max_wait}s")
    return False

def main():
    """Main WoL sequence."""
    logger.info("=== Docker Pre-Build WoL Script ===")
    logger.info(f"Target: {XEON_IP} (MAC: {XEON_MAC})")
    logger.info(f"Broadcast: {BROADCAST_IP}:{WOL_PORT}")
    
    # Check if Xeon is already online
    if ping_host(XEON_IP):
        logger.info(f"✅ Xeon server {XEON_IP} is already online!")
        logger.info("🚀 Ready to build containers or start TrueNAS apps!")
        return
    
    logger.info(f"⏰ Xeon server {XEON_IP} is offline, sending WoL...")
    
    # Send WoL packet
    if not send_wol_packet(XEON_MAC):
        logger.error("❌ Failed to send WoL packet!")
        logger.warning("⚠️ Continuing anyway - server might still wake up...")
    
    # Wait for server to respond
    if wait_for_server(XEON_IP):
        logger.info("🎉 Xeon server is now online!")
        logger.info("🚀 Ready to build containers or start TrueNAS apps!")
    else:
        logger.warning("⚠️ Server did not respond, but continuing...")
        logger.info("💡 You may want to check server status manually")
    
    logger.info("=== WoL sequence completed ===")

if __name__ == "__main__":
    main()
