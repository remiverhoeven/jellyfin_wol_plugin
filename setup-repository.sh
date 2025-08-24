#!/bin/bash

# Setup script for WoL Waker Plugin Repository
# This script helps set up the GitHub Pages repository

echo "🚀 Setting up WoL Waker Plugin Repository..."
echo "=============================================="

# Check if we're in the right directory
if [ ! -f "manifest.json" ]; then
    echo "❌ Error: manifest.json not found. Please run this script from the repository root."
    exit 1
fi

echo "✅ Repository structure verified"
echo ""
echo "📋 Next steps:"
echo "1. Create a new GitHub repository named 'jellyfin-wolwaker-plugin'"
echo "2. Push this code to the main branch"
echo "3. Enable GitHub Pages in repository settings:"
echo "   - Source: Deploy from a branch"
echo "   - Branch: gh-pages"
echo "   - Folder: / (root)"
echo "4. Add the repository URL to Jellyfin:"
echo "   Dashboard → Plugins → Repositories"
echo "   URL: https://remiverhoeven.github.io/jellyfin-wolwaker-plugin/"
echo ""
echo "🎯 After setup, users can install your plugin directly from Jellyfin!"
echo "🔄 Future updates: just push to main branch and GitHub Actions will auto-deploy"
echo ""
echo "📚 For more info, see: https://jellyfin.org/docs/general/development/plugins/"
