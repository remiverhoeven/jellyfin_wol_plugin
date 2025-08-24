# 🚀 WoL Waker Plugin Repository

This is a Jellyfin plugin repository for the WoL Waker plugin, hosted from the main plugin development repository.

## 📦 Installation

### Option 1: Add as Repository (Recommended)
1. Go to Jellyfin Dashboard → Plugins → Repositories
2. Click "Add Repository"
3. Enter: `https://remiverhoeven.github.io/jellyfin_wol_plugin_new/`
4. Click "Add"
5. Go to Plugins → Available
6. Find "WoL Waker" and click Install

### Option 2: Manual Installation
1. Download the plugin ZIP from the releases
2. Extract to your Jellyfin plugins directory
3. Restart Jellyfin

## 🔌 Plugin Features

- **Wake-on-LAN**: Automatically wake archival storage servers
- **Power Monitoring**: Integration with power monitoring APIs
- **Smart Wake Logic**: Prevents unnecessary wake attempts
- **User Notifications**: Keep users informed of server status

## 📋 Requirements

- Jellyfin 10.9.0 or higher
- .NET 8.0 runtime
- Wake-on-LAN capable network card

## 🆘 Support

For issues or questions, please open an issue on the main plugin repository.

## 📝 License

This plugin is provided as-is for personal use.

## 🔄 Auto-Updates

This plugin repository automatically updates when new versions are pushed to the main repository. Users just need to restart Jellyfin to get the latest version.
