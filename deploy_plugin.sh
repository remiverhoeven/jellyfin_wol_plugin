#!/bin/bash

echo "🚀 Deploying Updated WoL Waker Plugin..."

# Build the plugin
echo "📦 Building plugin..."
dotnet build Jellyfin.Plugin.WolWaker/Jellyfin.Plugin.WolWaker.csproj -c Release

if [ $? -ne 0 ]; then
    echo "❌ Build failed!"
    exit 1
fi

echo "✅ Build successful!"

# Create plugin package directory
PLUGIN_DIR="plugin-packages/WoLWaker"
mkdir -p "$PLUGIN_DIR"

# Copy plugin files
echo "📁 Copying plugin files..."
cp Jellyfin.Plugin.WolWaker/bin/Release/net8.0/Jellyfin.Plugin.WolWaker.dll "$PLUGIN_DIR/"
cp Jellyfin.Plugin.WolWaker/Web/wolwaker.html "$PLUGIN_DIR/"

# Create meta.json
echo "📝 Creating meta.json..."
cat > "$PLUGIN_DIR/meta.json" << EOF
{
  "category": "Utilities",
  "guid": "0ee23b2e-9d4d-4b5e-a0b9-7b4e54c5a5f2",
  "name": "WoL Waker",
  "description": "Automatically wakes up archival storage servers when media is requested, enabling power-efficient operation.",
  "owner": "Remi Verhoeven",
  "overview": "Wake-on-LAN plugin for Jellyfin that automatically wakes remote media servers when content is accessed.",
  "version": "0.1.1.0",
  "targetAbi": "10.9.0.0",
  "framework": "net8.0"
}
EOF

# Create ZIP package
echo "🗜️ Creating ZIP package..."
cd plugin-packages
zip -r "WoLWaker_0.1.1.0.zip" WoLWaker/
cd ..

# Calculate checksum
echo "🔍 Calculating checksum..."
CHECKSUM=$(md5 plugin-packages/WoLWaker_0.1.1.0.zip | awk '{print $4}')

echo "✅ Plugin deployed successfully!"
echo "📦 Package: plugin-packages/WoLWaker_0.1.1.0.zip"
echo "🔢 Checksum: $CHECKSUM"
echo ""
echo "🚀 Next steps:"
echo "1. Update your plugin repository manifest.json with:"
echo "   - version: '0.1.1.0'"
echo "   - checksum: '$CHECKSUM'"
echo "2. Deploy to GitHub Pages"
echo "3. Update the plugin in Jellyfin"
echo ""
echo "🎯 This version includes automatic WoL triggers for remote media access!"
