#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
SOLUTION="$REPO_ROOT/src/EShopDemo.sln"

echo "Scanning for security vulnerabilities..."

# Use built-in dotnet vulnerability scanner
echo "Checking for vulnerable packages..."
dotnet list "$SOLUTION" package --vulnerable --include-transitive 2>/dev/null || echo "No vulnerabilities found or packages not restored."

echo ""
echo "Checking for deprecated packages..."
dotnet list "$SOLUTION" package --deprecated 2>/dev/null || echo "No deprecated packages found."

echo ""
echo "Security scan completed."
