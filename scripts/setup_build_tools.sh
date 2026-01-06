#!/bin/bash
set -e

echo "🔧 Setting up Ducky.Sdk build tools..."

# Get the directory where this script is located
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

cd "$PROJECT_ROOT"

# Install dotnet-script
if ! command -v dotnet-script &> /dev/null; then
    echo "📦 Installing dotnet-script..."
    dotnet tool install --global dotnet-script --version 1.5.0
else
    echo "✅ dotnet-script already installed"
    dotnet-script --version
fi

# Install dotnet-ilrepack
if ! command -v dotnet-ilrepack &> /dev/null; then
    echo "📦 Installing dotnet-ilrepack..."
    dotnet tool install --global dotnet-ilrepack --version 2.0.44
else
    echo "✅ dotnet-ilrepack already installed"
    dotnet-ilrepack --version 2>/dev/null || echo " (version check not available)"
fi

# Check if .dotnet/tools is in PATH
if [[ ":$PATH:" != *":$HOME/.dotnet/tools:"* ]]; then
    echo ""
    echo "⚠️  WARNING: $HOME/.dotnet/tools is not in your PATH"
    echo ""
    echo "Add the following to your ~/.bashrc or ~/.zshrc:"
    echo "  export PATH=\"\$PATH:\$HOME/.dotnet/tools\""
    echo ""
    echo "Then run: source ~/.bashrc (or ~/.zshrc)"
fi

echo ""
echo "✨ Build tools setup complete!"
