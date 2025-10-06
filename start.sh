#!/bin/bash

set -e

echo "🎬 Starting Hive Movie Platform..."

# Check if shared directory exists
if [ ! -d "./shared" ]; then
    echo "📁 Creating shared directory..."
    mkdir -p ./shared
    echo "⚠️  Please mount your movie library to ./shared before starting containers"
    echo "   Example: mount your network drive to $(pwd)/shared"
fi

# Check if shared directory has content
if [ -z "$(ls -A ./shared 2>/dev/null)" ]; then
    echo "⚠️  Warning: ./shared directory is empty"
    echo "   Make sure to mount your movie library before starting the watcher service"
fi

# Build and start Docker services
echo "🐳 Building and starting Docker containers..."
podman compose up --build -d

echo ""
echo "🚀 Hive Movie Platform started successfully!"
echo ""
echo "📋 Service URLs:"
echo "   • Web Interface: http://localhost:8000"
echo "   • Movies API: http://localhost:8080" 
echo "   • Identity API: http://localhost:8082"
echo ""
echo "🔧 Environment Configuration:"
echo "   • AUTH_BASE_URL: http://hive-idm:8082 (internal Docker network)"
echo "   • MOVIES_BASE_URL: http://hive-app:8080 (internal Docker network)"
echo ""
echo "📁 Shared directory: $(pwd)/shared"
echo ""
echo "📊 Container Status:"
podman compose ps

echo ""
echo "🔍 To view logs:"
echo "   podman compose logs -f [service-name]"
echo ""
echo "🛑 To stop the platform:"
echo "   podman compose down"