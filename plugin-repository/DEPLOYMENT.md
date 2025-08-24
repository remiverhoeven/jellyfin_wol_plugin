# 🚀 Deployment Guide - WoL Waker Plugin Repository

## 📋 What We've Created

This repository contains everything needed to set up a Jellyfin plugin repository that users can add to their Jellyfin instances for automatic plugin installation and updates.

## 🎯 Repository Structure

```
github-pages-repo/
├── .github/workflows/build-and-deploy.yml  # Automated build pipeline
├── manifest.json                           # Plugin repository manifest
├── plugin-packages/                        # Plugin ZIP packages
│   ├── WoLWaker/                          # Plugin files
│   └── WoLWaker@v0.1.0.0.zip            # Current version
├── README.md                               # User documentation
├── setup-repository.sh                     # Setup helper script
└── DEPLOYMENT.md                           # This file
```

## 🚀 Setup Steps

### 1. Create GitHub Repository
```bash
# Create new repository on GitHub named 'jellyfin-wolwaker-plugin'
# Make it public (required for GitHub Pages)
```

### 2. Push Code to GitHub
```bash
git init
git add .
git commit -m "Initial commit: WoL Waker Plugin Repository"
git branch -M main
git remote add origin https://github.com/remiverhoeven/jellyfin-wolwaker-plugin.git
git push -u origin main
```

### 3. Enable GitHub Pages
- Go to repository Settings → Pages
- Source: "Deploy from a branch"
- Branch: `gh-pages`
- Folder: `/ (root)`
- Click Save

### 4. Enable GitHub Actions
- Go to repository Actions
- Click "I understand my workflows, go ahead and enable them"
- The workflow will run automatically on pushes to main

## 🔄 How It Works

### For Users
1. **Add Repository**: Dashboard → Plugins → Repositories → Add: `https://remiverhoeven.github.io/jellyfin-wolwaker-plugin/`
2. **Install Plugin**: Plugins → Available → Find "WoL Waker" → Install
3. **Auto-Updates**: Plugin updates automatically when you restart Jellyfin

### For Developers
1. **Make Changes**: Edit plugin code
2. **Push to Main**: `git push origin main`
3. **Auto-Build**: GitHub Actions builds and deploys automatically
4. **Auto-Update**: Users get updates on next Jellyfin restart

## 📦 Plugin Package Structure

Each plugin version includes:
- `Jellyfin.Plugin.WolWaker.dll` - Compiled plugin
- `wolwaker.html` - Configuration page
- `meta.json` - Plugin metadata

## 🎯 Benefits

- ✅ **Professional Distribution**: Like official Jellyfin plugins
- ✅ **Auto-Updates**: Users get updates automatically
- ✅ **Easy Installation**: One-click install from Jellyfin
- ✅ **Version Management**: Automatic version tracking
- ✅ **Automated Builds**: GitHub Actions handles everything

## 🆘 Troubleshooting

### GitHub Pages Not Working
- Ensure repository is public
- Check GitHub Pages settings
- Wait a few minutes for deployment

### Plugin Not Appearing
- Verify repository URL is correct
- Check manifest.json format
- Ensure ZIP package is accessible

### Build Failures
- Check GitHub Actions logs
- Verify .NET 8.0 compatibility
- Check file paths in workflow

## 📚 Resources

- [Jellyfin Plugin Development](https://jellyfin.org/docs/general/development/plugins/)
- [GitHub Pages Documentation](https://docs.github.com/en/pages)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
