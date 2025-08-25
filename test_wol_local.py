#!/usr/bin/env python3
"""
Local Wake-on-LAN test script to verify packet format and sending
"""
import socket
import binascii

def create_magic_packet(mac_address):
    """Create a Wake-on-LAN magic packet."""
    # Remove separators and convert to uppercase
    clean_mac = mac_address.replace(":", "").replace("-", "").replace(".", "").upper()
    
    if len(clean_mac) != 12:
        raise ValueError(f"Invalid MAC address length: {len(clean_mac)}")
    
    # Convert to bytes
    mac_bytes = binascii.unhexlify(clean_mac)
    
    # Magic packet: 6 bytes of 0xFF + MAC address repeated 16 times
    packet = b'\xff' * 6 + mac_bytes * 16
    
    return packet

def send_wol_packet(mac_address, broadcast_ip="255.255.255.255", port=9):
    """Send Wake-on-LAN packet."""
    try:
        packet = create_magic_packet(mac_address)
        
        print(f"Creating WoL packet for MAC: {mac_address}")
        print(f"Packet size: {len(packet)} bytes")
        print(f"Packet hex: {packet.hex()}")
        
        # Create UDP socket
        with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as sock:
            sock.setsockopt(socket.SOL_SOCKET, socket.SO_BROADCAST, 1)
            
            # Send packet
            result = sock.sendto(packet, (broadcast_ip, port))
            print(f"Sent {result} bytes to {broadcast_ip}:{port}")
            
        return True
        
    except Exception as e:
        print(f"Error: {e}")
        return False

if __name__ == "__main__":
    # Test with your MAC address
    mac = "a4:ae:11:19:ac:33"
    
    print("=== Local Wake-on-LAN Test ===")
    print(f"Target MAC: {mac}")
    print(f"Broadcast: 255.255.255.255:9")
    print()
    
    success = send_wol_packet(mac)
    
    if success:
        print("✅ WoL packet sent successfully!")
        print("Check if target machine wakes up...")
    else:
        print("❌ Failed to send WoL packet")
