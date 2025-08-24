#!/bin/bash

# Local Development Setup Script for Jellyfin Wake-on-LAN Plugin
# This script sets up a local Jellyfin instance for plugin development and testing

set -e

echo "🚀 Setting up local Jellyfin development environment..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if running on macOS
if [[ "$OSTYPE" == "darwin"* ]]; then
    PLATFORM="macos"
    print_status "Detected macOS platform"
elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
    PLATFORM="linux"
    print_status "Detected Linux platform"
else
    print_error "Unsupported platform: $OSTYPE"
    exit 1
fi

# Check prerequisites
check_prerequisites() {
    print_status "Checking prerequisites..."
    
    # Check if .NET 8.0 is installed
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET 8.0 SDK is not installed"
        print_status "Please install .NET 8.0 SDK from: https://dotnet.microsoft.com/download"
        exit 1
    fi
    
    DOTNET_VERSION=$(dotnet --version)
    if [[ ! "$DOTNET_VERSION" == 8.* ]]; then
        print_error "Wrong .NET version: $DOTNET_VERSION (need 8.x)"
        exit 1
    fi
    
    print_success ".NET $DOTNET_VERSION found"
    
    # Check if Docker is available (optional)
    if command -v docker &> /dev/null; then
        print_success "Docker found (will use for Jellyfin)"
        USE_DOCKER=true
    else
        print_warning "Docker not found, will use native installation"
        USE_DOCKER=false
    fi
    
    # Check if Homebrew is available (macOS)
    if [[ "$PLATFORM" == "macos" ]] && command -v brew &> /dev/null; then
        print_success "Homebrew found"
        USE_HOMEBREW=true
    else
        USE_HOMEBREW=false
    fi
}

# Install Jellyfin
install_jellyfin() {
    print_status "Installing Jellyfin..."
    
    if [[ "$USE_DOCKER" == true ]]; then
        install_jellyfin_docker
    elif [[ "$PLATFORM" == "macos" && "$USE_HOMEBREW" == true ]]; then
        install_jellyfin_homebrew
    else
        install_jellyfin_manual
    fi
}

install_jellyfin_docker() {
    print_status "Installing Jellyfin using Docker..."
    
    # Check if Jellyfin container already exists
    if docker ps -a --format "table {{.Names}}" | grep -q "jellyfin-dev"; then
        print_warning "Jellyfin container already exists, removing..."
        docker rm -f jellyfin-dev
    fi
    
    # Create directories for Jellyfin
    mkdir -p ~/jellyfin-dev-config
    mkdir -p ~/test-media
    
    # Run Jellyfin container
    docker run -d \
        --name jellyfin-dev \
        -p 8096:8096 \
        -v ~/jellyfin-dev-config:/config \
        -v ~/test-media:/media \
        jellyfin/jellyfin:latest
    
    print_success "Jellyfin container started"
    print_status "Access Jellyfin at: http://localhost:8096"
}

install_jellyfin_homebrew() {
    print_status "Installing Jellyfin using Homebrew..."
    
    if ! brew list jellyfin &> /dev/null; then
        brew install jellyfin
    fi
    
    # Start Jellyfin service
    brew services start jellyfin
    
    print_success "Jellyfin installed and started via Homebrew"
    print_status "Access Jellyfin at: http://localhost:8096"
}

install_jellyfin_manual() {
    print_status "Installing Jellyfin manually..."
    
    if [[ "$PLATFORM" == "macos" ]]; then
        print_status "Please download Jellyfin from: https://jellyfin.org/downloads/"
        print_status "Extract and run the application"
    else
        print_status "Please install Jellyfin using your package manager or download from: https://jellyfin.org/downloads/"
    fi
    
    print_warning "Manual installation requires additional setup steps"
}

# Setup plugin development environment
setup_plugin_dev() {
    print_status "Setting up plugin development environment..."
    
    # Navigate to plugin directory
    cd "$(dirname "$0")/Jellyfin.Plugin.WolWaker"
    
    # Restore dependencies
    print_status "Restoring .NET dependencies..."
    dotnet restore
    
    # Build plugin
    print_status "Building plugin..."
    dotnet build -c Debug
    
    print_success "Plugin built successfully"
}

# Install plugin to local Jellyfin
install_plugin() {
    print_status "Installing plugin to local Jellyfin..."
    
    # Determine Jellyfin plugins directory
    if [[ "$USE_DOCKER" == true ]]; then
        PLUGIN_DIR="~/jellyfin-dev-config/plugins/WoLWaker"
    elif [[ "$PLATFORM" == "macos" ]]; then
        PLUGIN_DIR="$HOME/Library/Application Support/Jellyfin-Server/plugins/WoLWaker"
    else
        PLUGIN_DIR="$HOME/.local/share/jellyfin/plugins/WoLWaker"
    fi
    
    # Create plugin directory
    mkdir -p "$PLUGIN_DIR"
    
    # Copy plugin files
    cp bin/Debug/net8.0/Jellyfin.Plugin.WolWaker.dll "$PLUGIN_DIR/"
    
    print_success "Plugin installed to: $PLUGIN_DIR"
}

# Create test configuration
create_test_config() {
    print_status "Creating test configuration..."
    
    # Create a basic test configuration file
    cat > test-config.json << EOF
{
  "MacAddress": "00:11:22:33:44:55",
  "BroadcastAddress": "255.255.255.255",
  "ServerIp": "127.0.0.1",
  "BroadcastPort": 9,
  "WakeTimeout": 300,
  "CheckInterval": 10,
  "EnableAutoWake": true,
  "ShowUserMessages": true,
  "CooldownSeconds": 300,
  "PowerMonitoringEnabled": false,
  "PowerMonitorApiUrl": "http://localhost:5678/api/power-status",
  "PowerMonitorPollInterval": 5
}
EOF
    
    print_success "Test configuration created: test-config.json"
}

# Setup test media
setup_test_media() {
    print_status "Setting up test media..."
    
    TEST_MEDIA_DIR="$HOME/test-media"
    mkdir -p "$TEST_MEDIA_DIR"
    
    # Create sample directory structure
    mkdir -p "$TEST_MEDIA_DIR/Movies"
    mkdir -p "$TEST_MEDIA_DIR/TV Shows"
    
    # Create a sample text file to simulate media
    echo "This is a test media file for Jellyfin plugin development" > "$TEST_MEDIA_DIR/Movies/sample-movie.txt"
    echo "This is a test TV show file for Jellyfin plugin development" > "$TEST_MEDIA_DIR/TV Shows/sample-show.txt"
    
    print_success "Test media created in: $TEST_MEDIA_DIR"
    print_status "You can add real media files here for testing"
}

# Main setup function
main() {
    echo "🎬 Jellyfin Wake-on-LAN Plugin - Local Development Setup"
    echo "========================================================"
    echo ""
    
    check_prerequisites
    install_jellyfin
    setup_plugin_dev
    install_plugin
    create_test_config
    setup_test_media
    
    echo ""
    echo "🎉 Setup complete! Here's what to do next:"
    echo ""
    echo "1. 🌐 Open Jellyfin in your browser: http://localhost:8096"
    echo "2. ⚙️  Complete Jellyfin initial setup"
    echo "3. 📁 Add test media libraries pointing to: $HOME/test-media"
    echo "4. 🔌 Check that the WoL Waker plugin appears in Dashboard → Plugins"
    echo "5. ⚡ Configure the plugin with your test settings"
    echo "6. 🧪 Test the plugin functionality"
    echo ""
    echo "📚 For development workflow, see: DEVELOPMENT.md"
    echo "🐛 For troubleshooting, see the debugging section in DEVELOPMENT.md"
    echo ""
    echo "Happy coding! 🚀"
}

# Run main function
main "$@"
