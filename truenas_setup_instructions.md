# TrueNAS Xeon WoL Startup Setup

## 📋 **Setup Instructions:**

### **Step 1: Install the Script**
```bash
# Copy script to TrueNAS
sudo cp truenas_wol_startup.py /root/scripts/
sudo chmod +x /root/scripts/truenas_wol_startup.py

# Create log directory
sudo mkdir -p /var/log
sudo touch /var/log/xeon_wol_startup.log
```

### **Step 2: Create TrueNAS Init Task**

**Via TrueNAS Web UI:**
1. **System** → **Advanced** → **Init/Shutdown Scripts**
2. **Add** new script
3. **Type**: `Script`
4. **Script**: `/root/scripts/truenas_wol_startup.py`
5. **When**: `Pre Init`
6. **Enabled**: ✅
7. **Timeout**: `600` (10 minutes)

### **Step 3: Alternative - Cron Task**

**For manual control:**
1. **Tasks** → **Cron Jobs** → **Add**
2. **Command**: `/root/scripts/truenas_wol_startup.py`
3. **Schedule**: `@reboot` (runs on boot)
4. **User**: `root`
5. **Enabled**: ✅

### **Step 4: Configure App Auto-Start**

**Disable auto-start for dependent apps:**
1. **Apps** → **Installed Applications**
2. For each app (**Jellyfin**, **Radarr**, **Sonarr**):
   - **Edit** → **Advanced Settings**
   - **Disable**: "Start on system boot"
   - **Save**

This prevents conflicts with the script.

## 🧪 **Testing:**

### **Manual Test:**
```bash
sudo /root/scripts/truenas_wol_startup.py
```

### **Check Logs:**
```bash
tail -f /var/log/xeon_wol_startup.log
```

### **Verify Apps Started:**
```bash
midclt call app.query | grep -E "(jellyfin|radarr|sonarr)"
```

## ⚙️ **Customization:**

Edit the script to modify:
- **XEON_MAC**: Your server's MAC address
- **XEON_IP**: Your server's IP address  
- **BROADCAST_IP**: Your network broadcast address
- **DEPENDENT_APPS**: Apps to start after WoL
- **MAX_WAIT_TIME**: How long to wait for server

## 🔄 **How It Works:**

1. **Script runs** at TrueNAS boot
2. **Checks** if Xeon is online (ping)
3. **Sends WoL** if Xeon is offline
4. **Waits** for Xeon to respond
5. **Starts** dependent apps in sequence
6. **Logs** everything for debugging

## 🎯 **Benefits:**

✅ **Solves the dependency loop**  
✅ **Automatic Xeon wake-up**  
✅ **Proper startup order**  
✅ **Works for multiple apps**  
✅ **Detailed logging**  
✅ **Graceful error handling**  

Your apps will now start reliably even when Xeon is powered down! 🚀
