#!/bin/bash

# Count lines of code in the eshop-demo project
# Usage: ./tools/count-lines.sh

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"

cd "$ROOT_DIR"

echo "=================================="
echo "   Lines of Code Statistics"
echo "=================================="
echo ""

# Directories to exclude
EXCLUDE_DIRS="bin|obj|node_modules|.git|Migrations"

# Count C# files
cs_lines=$(find src tests -name "*.cs" -type f 2>/dev/null | grep -Ev "($EXCLUDE_DIRS)" | xargs wc -l 2>/dev/null | tail -1 | awk '{print $1}')
cs_files=$(find src tests -name "*.cs" -type f 2>/dev/null | grep -Ev "($EXCLUDE_DIRS)" | wc -l | tr -d ' ')

# Count Proto files
proto_lines=$(find src -name "*.proto" -type f 2>/dev/null | grep -Ev "($EXCLUDE_DIRS)" | xargs wc -l 2>/dev/null | tail -1 | awk '{print $1}')
proto_files=$(find src -name "*.proto" -type f 2>/dev/null | grep -Ev "($EXCLUDE_DIRS)" | wc -l | tr -d ' ')

# Count JSON/YAML config files
config_lines=$(find src -name "*.json" -o -name "*.yaml" -o -name "*.yml" 2>/dev/null | grep -Ev "($EXCLUDE_DIRS)" | xargs wc -l 2>/dev/null | tail -1 | awk '{print $1}')
config_files=$(find src -name "*.json" -o -name "*.yaml" -o -name "*.yml" 2>/dev/null | grep -Ev "($EXCLUDE_DIRS)" | wc -l | tr -d ' ')

# Count csproj files
csproj_lines=$(find src tests -name "*.csproj" -type f 2>/dev/null | xargs wc -l 2>/dev/null | tail -1 | awk '{print $1}')
csproj_files=$(find src tests -name "*.csproj" -type f 2>/dev/null | wc -l | tr -d ' ')

# Handle empty results
cs_lines=${cs_lines:-0}
proto_lines=${proto_lines:-0}
config_lines=${config_lines:-0}
csproj_lines=${csproj_lines:-0}

echo "C# Source Code:"
printf "  Files: %6s\n" "$cs_files"
printf "  Lines: %6s\n" "$cs_lines"
echo ""

echo "Proto Files:"
printf "  Files: %6s\n" "$proto_files"
printf "  Lines: %6s\n" "$proto_lines"
echo ""

echo "Config Files (JSON/YAML):"
printf "  Files: %6s\n" "$config_files"
printf "  Lines: %6s\n" "$config_lines"
echo ""

echo "Project Files (csproj):"
printf "  Files: %6s\n" "$csproj_files"
printf "  Lines: %6s\n" "$csproj_lines"
echo ""

total_lines=$((cs_lines + proto_lines + config_lines + csproj_lines))
total_files=$((cs_files + proto_files + config_files + csproj_files))

echo "=================================="
printf "TOTAL:  %6s files, %s lines\n" "$total_files" "$total_lines"
echo "=================================="

echo ""
echo "Top 10 largest C# files:"
echo "--------------------------"
find src tests -name "*.cs" -type f 2>/dev/null | grep -Ev "($EXCLUDE_DIRS)" | xargs wc -l 2>/dev/null | sort -rn | head -11 | tail -10
