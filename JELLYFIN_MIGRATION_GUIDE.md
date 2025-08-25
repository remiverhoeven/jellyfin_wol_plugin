# Jellyfin Migration Guide: TrueNAS App → Docker Compose

## 🎯 **What This Achieves:**

✅ **Fixes WoL issue** - Host networking enables proper UDP broadcasts  
✅ **Preserves all data** - Libraries, users, settings, watch history  
✅ **Better control** - Manage via Dockage with proper networking  
✅ **No data loss** - Uses existing config paths  

## 📋 **Prerequisites:**

- **Dockage** installed and working
- **Current Jellyfin config** backed up
- **TrueNAS Jellyfin app** accessible (to stop it)

## 🚀 **Migration Steps:**

### **Step 1: Backup Current Config**
```bash
# Your config is already in:
/mnt/n100pool/apps/jellyfin/config/
/mnt/n100pool/apps/jellyfin/cache/
```

### **Step 2: Stop TrueNAS Jellyfin App**
1. **Go to TrueNAS** → **Apps** → **Installed Applications**
2. **Find Jellyfin** → **Stop** (don't delete yet!)
3. **Wait for it to fully stop**

### **Step 3: Deploy Docker Jellyfin**
1. **Open Dockage**
2. **Create New Stack** named `jellyfin`
3. **Copy the compose file** content
4. **Deploy the stack**

### **Step 4: Verify Migration**
1. **Wait for container to start**
2. **Access Jellyfin** at `http://your-truenas-ip:8096`
3. **Check libraries** - should be identical
4. **Check users** - all accounts preserved
5. **Check settings** - everything migrated

### **Step 5: Test WoL Plugin**
1. **Go to Plugins** → **WoL Waker** → **Configuration**
2. **Click "Test Wake-on-LAN"**
3. **Should work now!** 🎉

### **Step 6: Clean Up (Optional)**
1. **Verify Docker Jellyfin works perfectly**
2. **Delete TrueNAS Jellyfin app** (frees up resources)
3. **Keep config backup** just in case

## 🔧 **Docker Compose Features:**

- **Host networking** - WoL packets work properly
- **Same user/group IDs** - File permissions preserved
- **All volume mounts** - Local and remote storage
- **Health checks** - Automatic monitoring
- **Resource limits** - Prevent memory issues
- **Auto-restart** - Survives reboots

## 📁 **Volume Mapping:**

| Container Path | Host Path | Purpose |
|----------------|-----------|---------|
| `/config` | `/mnt/n100pool/apps/jellyfin/config` | Settings, users, libraries |
| `/cache` | `/mnt/n100pool/apps/jellyfin/cache` | Transcoding cache |
| `/media/local` | `/mnt/n100pool/media` | Local media |
| `/media/remote` | `/mnt/n100pool/remotes/xeon_media` | Xeon media (remote) |

## 🚨 **Important Notes:**

- **Port 8096** will be used by Docker Jellyfin
- **Host networking** gives full network access
- **User ID 568** preserved for file permissions
- **All data** remains in place

## 🎉 **Expected Results:**

1. **Jellyfin works exactly the same** (no data loss)
2. **WoL plugin works** (host networking fixed it)
3. **Better performance** (Docker optimization)
4. **Easier management** (via Dockage)

## 🔍 **Troubleshooting:**

### **If Jellyfin won't start:**
- Check if **port 8096** is free
- Verify **config paths** exist
- Check **user permissions** (568:568)

### **If libraries missing:**
- Verify **volume mounts** are correct
- Check **file permissions** on media paths
- Restart container

### **If WoL still doesn't work:**
- Confirm **host networking** is enabled
- Check **Jellyfin logs** for network errors
- Verify **MAC address** and **broadcast IP**

**Ready to migrate?** This should give you a working WoL plugin while preserving everything! 🚀
