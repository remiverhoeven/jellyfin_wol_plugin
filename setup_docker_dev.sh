#!/bin/bash

# Docker Development Setup Script for Jellyfin Wake-on-LAN Plugin
# This script sets up a complete development environment using Docker that
# closely mirrors your TrueNAS Scale production setup

set -e

echo "🐳 Setting up Docker-based Jellyfin development environment..."
echo "This will create an environment similar to your TrueNAS Scale setup"

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

# Check prerequisites
check_prerequisites() {
    print_status "Checking prerequisites..."
    
    # Check if Docker is installed
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed"
        print_status "Please install Docker Desktop from: https://www.docker.com/products/docker-desktop"
        exit 1
    fi
    
    # Check if Docker is running
    if ! docker info &> /dev/null; then
        print_error "Docker is not running"
        print_status "Please start Docker Desktop and try again"
        exit 1
    fi
    
    print_success "Docker is running"
    
    # Check if Docker Compose is available
    if ! command -v docker-compose &> /dev/null; then
        print_warning "Docker Compose not found, using 'docker compose' (newer Docker versions)"
        DOCKER_COMPOSE_CMD="docker compose"
    else
        DOCKER_COMPOSE_CMD="docker-compose"
    fi
    
    # Check if .NET 8.0 is installed
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET 8.0 SDK is not installed"
        print_status "Please install .NET 8.0 SDK from: https://dotnet.microsoft.com/download"
        exit 1
    fi
    
    # Use the specific .NET 8.0 installation
    DOTNET_CMD="/opt/homebrew/Cellar/dotnet@8/8.0.119/bin/dotnet"
    if [ ! -f "$DOTNET_CMD" ]; then
        print_error ".NET 8.0 SDK not found at expected path: $DOTNET_CMD"
        exit 1
    fi
    
    DOTNET_VERSION=$("$DOTNET_CMD" --version)
    if [[ ! "$DOTNET_VERSION" == 8.* ]]; then
        print_error "Wrong .NET version: $DOTNET_VERSION (need 8.x)"
        exit 1
    fi
    
    print_success ".NET $DOTNET_VERSION found at $DOTNET_CMD"
}

# Create development directory structure
create_directories() {
    print_status "Creating development directory structure..."
    
    # Create necessary directories
    mkdir -p jellyfin-config
    mkdir -p n8n-data
    mkdir -p n8n-workflows
    mkdir -p test-media
    mkdir -p test-media-archival
    mkdir -p mock-xeon
    mkdir -p power-monitor-sim
    
    print_success "Directory structure created"
}

# Setup test media
setup_test_media() {
    print_status "Setting up test media..."
    
    # Create sample media structure
    mkdir -p test-media/Movies
    mkdir -p test-media/TV\ Shows
    mkdir -p test-media-archival/Movies
    mkdir -p test-media-archival/TV\ Shows
    
    # Create sample files for local media (N100)
    echo "This is a test movie file for local media (N100)" > "test-media/Movies/local-movie.txt"
    echo "This is a test TV show file for local media (N100)" > "test-media/TV Shows/local-show.txt"
    
    # Create sample files for archival media (Xeon)
    echo "This is a test movie file for archival media (Xeon)" > "test-media-archival/Movies/archival-movie.txt"
    echo "This is a test TV show file for archival media (Xeon)" > "test-media-archival/TV Shows/archival-show.txt"
    
    print_success "Test media created"
}

# Setup mock Xeon server
setup_mock_xeon() {
    print_status "Setting up mock Xeon server..."
    
    # Create a simple HTML page for the mock server
    cat > mock-xeon/index.html << 'EOF'
<!DOCTYPE html>
<html>
<head>
    <title>Mock Xeon Server</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; }
        .status { padding: 20px; background: #f0f0f0; border-radius: 5px; }
        .online { background: #d4edda; color: #155724; }
        .offline { background: #f8d7da; color: #721c24; }
    </style>
</head>
<body>
    <h1>Mock Xeon Server</h1>
    <p>This simulates your Xeon server for development testing.</p>
    
    <div class="status" id="status">
        <h2>Server Status</h2>
        <p><strong>State:</strong> <span id="state">Unknown</span></p>
        <p><strong>Power:</strong> <span id="power">Unknown</span> W</p>
        <p><strong>Network:</strong> <span id="network">Unknown</span></p>
        <p><strong>Services:</strong> <span id="services">Unknown</span></p>
    </div>
    
    <h2>Simulated Services</h2>
    <ul>
        <li>HTTP Server (Port 80)</li>
        <li>Jellyfin Media Server</li>
        <li>File Storage (Archival Media)</li>
    </ul>
    
    <script>
        function updateStatus() {
            fetch('http://localhost:5000/api/power-status/server')
                .then(response => response.json())
                .then(data => {
                    document.getElementById('state').textContent = data.power.state;
                    document.getElementById('power').textContent = data.power.wattage;
                    document.getElementById('network').textContent = data.network.reachable ? 'Online' : 'Offline';
                    document.getElementById('services').textContent = data.services.jellyfin ? 'Available' : 'Unavailable';
                    
                    const statusDiv = document.getElementById('status');
                    if (data.power.state === 'running' || data.power.state === 'idle') {
                        statusDiv.className = 'status online';
                    } else {
                        statusDiv.className = 'status offline';
                    }
                })
                .catch(error => {
                    console.error('Error fetching status:', error);
                    document.getElementById('state').textContent = 'Error';
                });
        }
        
        // Update status every 2 seconds
        setInterval(updateStatus, 2000);
        updateStatus();
    </script>
</body>
</html>
EOF
    
    print_success "Mock Xeon server setup complete"
}

# Start Docker services
start_services() {
    print_status "Starting Docker services..."
    
    # Stop any existing services
    print_status "Stopping existing services..."
    $DOCKER_COMPOSE_CMD down 2>/dev/null || true
    
    # Start services
    print_status "Starting Jellyfin, n8n, and mock services..."
    $DOCKER_COMPOSE_CMD up -d
    
    # Wait for services to be ready
    print_status "Waiting for services to be ready..."
    sleep 10
    
    # Check service status
    print_status "Checking service status..."
    $DOCKER_COMPOSE_CMD ps
    
    print_success "Docker services started"
}

# Setup plugin development environment
setup_plugin_dev() {
    print_status "Setting up plugin development environment..."
    
    # Navigate to plugin directory
    cd "$(dirname "$0")/Jellyfin.Plugin.WolWaker"
    
    # Restore dependencies
    print_status "Restoring .NET dependencies..."
    "$DOTNET_CMD" restore
    
    # Build plugin
    print_status "Building plugin..."
    "$DOTNET_CMD" build -c Debug
    
    print_success "Plugin built successfully"
}

# Install plugin to Docker Jellyfin
install_plugin() {
    print_status "Installing plugin to Docker Jellyfin..."
    
    # Copy plugin to Jellyfin config directory
    PLUGIN_DIR="../jellyfin-config/plugins/WoLWaker"
    mkdir -p "$PLUGIN_DIR"
    
    # Copy plugin files
    cp bin/Debug/net8.0/Jellyfin.Plugin.WolWaker.dll "$PLUGIN_DIR/"
    
    print_success "Plugin installed to Docker Jellyfin"
    
    # Return to original directory
    cd ..
}

# Create test configuration
create_test_config() {
    print_status "Creating test configuration..."
    
    # Create a test configuration file for the plugin
    cat > test-config.json << EOF
{
  "MacAddress": "00:11:22:33:44:55",
  "BroadcastAddress": "255.255.255.255",
  "ServerIp": "172.20.0.3",
  "BroadcastPort": 9,
  "WakeTimeout": 300,
  "CheckInterval": 10,
  "EnableAutoWake": true,
  "ShowUserMessages": true,
  "CooldownSeconds": 300,
  "PowerMonitoringEnabled": true,
  "PowerMonitorApiUrl": "http://localhost:5001/api/power-status",
  "PowerMonitorPollInterval": 5
}
EOF
    
    print_success "Test configuration created: test-config.json"
}

# Test the setup
test_setup() {
    print_status "Testing the setup..."
    
    # Test Jellyfin
    print_status "Testing Jellyfin..."
    if curl -s http://localhost:8096 > /dev/null; then
        print_success "Jellyfin is accessible at http://localhost:8096"
    else
        print_warning "Jellyfin might not be ready yet, wait a few more seconds"
    fi
    
    # Test n8n
    print_status "Testing n8n..."
    if curl -s http://localhost:5678 > /dev/null; then
        print_success "n8n is accessible at http://localhost:5678"
    else
        print_warning "n8n might not be ready yet, wait a few more seconds"
    fi
    
    # Test power monitor simulator
    print_status "Testing power monitor simulator..."
    if curl -s http://localhost:5001/health > /dev/null; then
        print_success "Power monitor simulator is accessible at http://localhost:5001"
    else
        print_warning "Power monitor simulator might not be ready yet, wait a few more seconds"
    fi
    
    # Test mock Xeon server
    print_status "Testing mock Xeon server..."
    if curl -s http://localhost:8080 > /dev/null; then
        print_success "Mock Xeon server is accessible at http://localhost:8080"
    else
        print_warning "Mock Xeon server might not be ready yet, wait a few more seconds"
    fi
}

# Show next steps
show_next_steps() {
    echo ""
    echo "🎉 Docker development environment setup complete!"
    echo "================================================"
    echo ""
    echo "🌐 Services available:"
    echo "  - Jellyfin:     http://localhost:8096"
    echo "  - n8n:          http://localhost:5678"
    echo "  - Power Monitor: http://localhost:5001"
    echo "  - Mock Xeon:    http://localhost:8080"
    echo ""
    echo "📁 Test media locations:"
    echo "  - Local media (N100): ./test-media"
    echo "  - Archival media (Xeon): ./test-media-archival"
    echo ""
    echo "🔌 Plugin installed to: ./jellyfin-config/plugins/WoLWaker/"
    echo ""
    echo "📋 Next steps:"
    echo "1. 🌐 Open Jellyfin at http://localhost:8096 and complete setup"
    echo "2. 📁 Add media libraries pointing to test-media directories"
    echo "3. 🔌 Check that the WoL Waker plugin appears in Dashboard → Plugins"
    echo "4. ⚡ Configure the plugin with test-config.json settings"
    echo "5. 🧪 Test the plugin with the power monitor simulator"
    echo ""
    echo "🔄 To restart services:"
    echo "  $DOCKER_COMPOSE_CMD restart"
    echo ""
    echo "🛑 To stop services:"
    echo "  $DOCKER_COMPOSE_CMD down"
    echo ""
    echo "📚 For development workflow, see: DEVELOPMENT.md"
    echo "🐛 For troubleshooting, see the debugging section in DEVELOPMENT.md"
    echo ""
    echo "Happy coding! 🚀"
}

# Main setup function
main() {
    echo "🐳 Jellyfin Wake-on-LAN Plugin - Docker Development Setup"
    echo "========================================================="
    echo "This will create an environment similar to your TrueNAS Scale setup"
    echo ""
    
    check_prerequisites
    create_directories
    setup_test_media
    setup_mock_xeon
    start_services
    setup_plugin_dev
    install_plugin
    create_test_config
    test_setup
    show_next_steps
}

# Run main function
main "$@"
