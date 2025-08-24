# 🚀 WoL Waker Plugin - Production Deployment

## 📦 What's Included

- `Jellyfin.Plugin.WolWaker.dll` - The compiled plugin
- `meta.json` - Plugin metadata for Jellyfin

## 🎯 Simple Deployment

### Manual Installation (Recommended)
1. **Create plugin directory** on your Jellyfin server:
   ```bash
   mkdir -p /path/to/jellyfin/plugins/WoLWaker
   ```

2. **Copy these 2 files** to the directory:
   - `Jellyfin.Plugin.WolWaker.dll`
   - `meta.json`

3. **Set permissions** (adjust user/group as needed):
   ```bash
   chown -R jellyfin:jellyfin /path/to/jellyfin/plugins/WoLWaker
   chmod 755 /path/to/jellyfin/plugins/WoLWaker
   chmod 644 /path/to/jellyfin/plugins/WoLWaker/*
   ```

4. **Restart Jellyfin**

## 🔍 Common Plugin Directories

- **Linux (systemd)**: `/var/lib/jellyfin/plugins`
- **Linux (manual)**: `/opt/jellyfin/plugins`
- **Docker**: `/config/plugins` (mounted volume)
- **TrueNAS Scale**: Check your app configuration
- **Windows**: `C:\ProgramData\Jellyfin\Server\plugins`

## ✅ Verification

After deployment and restart:
1. Go to Jellyfin Dashboard → Plugins
2. Look for "WoL Waker" in the list
3. Click Configure to access settings

## 🆘 Troubleshooting

- **Plugin not visible**: Check file permissions and restart Jellyfin
- **Configuration page error**: Verify both DLL and meta.json are present
- **Permission denied**: Ensure Jellyfin user has read access to plugin directory

## 📋 Requirements

- Jellyfin 10.x
- .NET 8.0 runtime
- Wake-on-LAN capable network card
