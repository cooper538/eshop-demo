#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
SOLUTION="$REPO_ROOT/src/EShopDemo.sln"

echo "Checking for unused NuGet packages..."

# Ensure tools are restored
dotnet tool restore --tool-manifest "$REPO_ROOT/.config/dotnet-tools.json" > /dev/null 2>&1 || true

dotnet tool run dotnet-unused "$SOLUTION"
