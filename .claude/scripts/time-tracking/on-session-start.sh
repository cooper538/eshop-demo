#!/bin/bash
# Hook: SessionStart
# Purpose: Initialize session tracking - cache branch, commit, create temp directory
# Estimated execution: ~10-15ms

set -e

# Read JSON input from stdin
INPUT=$(cat)

# Extract session_id from JSON input
SESSION_ID=$(echo "$INPUT" | grep -o '"session_id":"[^"]*"' | cut -d'"' -f4)

# Fallback if parsing fails
if [ -z "$SESSION_ID" ]; then
    SESSION_ID="unknown-$$"
fi

# Create temp directory for this session
TEMP_DIR="/tmp/claude-timing-${SESSION_ID}"
mkdir -p "$TEMP_DIR"

# Cache git branch (expensive operation, do it once)
BRANCH=$(git branch --show-current 2>/dev/null || echo "no-branch")
echo "$BRANCH" > "$TEMP_DIR/branch"

# Cache base commit
COMMIT=$(git rev-parse --short HEAD 2>/dev/null || echo "no-commit")
echo "$COMMIT" > "$TEMP_DIR/base_commit"

# Initialize turn counter
echo "0" > "$TEMP_DIR/turn_id"

# Initialize tool counter
echo "0" > "$TEMP_DIR/tool_count"

exit 0
