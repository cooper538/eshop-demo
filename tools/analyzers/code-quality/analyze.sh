#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
SOLUTION="$REPO_ROOT/src/EShopDemo.sln"

echo "Checking code quality..."

# Check formatting with CSharpier (C# files only)
echo "Checking code formatting..."
dotnet csharpier check "$REPO_ROOT/src" --include-generated

# Run dotnet format analyzers (style + analyzers)
echo "Running Roslyn analyzers..."
dotnet format "$SOLUTION" --verify-no-changes --verbosity minimal || true

echo "Code quality check completed."
