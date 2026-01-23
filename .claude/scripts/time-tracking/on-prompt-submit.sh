#!/bin/bash
# Hook: UserPromptSubmit
# Purpose: Record start time, increment turn counter, reset tool counter
# Estimated execution: ~3-5ms

set -e

# Read JSON input from stdin
INPUT=$(cat)

# Extract session_id from JSON input
SESSION_ID=$(echo "$INPUT" | grep -o '"session_id":"[^"]*"' | cut -d'"' -f4)

# Fallback if parsing fails
if [ -z "$SESSION_ID" ]; then
    exit 0
fi

TEMP_DIR="/tmp/claude-timing-${SESSION_ID}"

# Check if temp dir exists (session might not have triggered SessionStart)
if [ ! -d "$TEMP_DIR" ]; then
    mkdir -p "$TEMP_DIR"
    # Initialize if SessionStart was missed
    BRANCH=$(git branch --show-current 2>/dev/null || echo "no-branch")
    echo "$BRANCH" > "$TEMP_DIR/branch"
    COMMIT=$(git rev-parse --short HEAD 2>/dev/null || echo "no-commit")
    echo "$COMMIT" > "$TEMP_DIR/base_commit"
    echo "0" > "$TEMP_DIR/turn_id"
fi

# Save start timestamp with milliseconds
date +%Y-%m-%dT%H:%M:%S.%3N > "$TEMP_DIR/start_time"

# Also save epoch with nanoseconds for precise duration calculation
date +%s.%N > "$TEMP_DIR/start_epoch"

# Increment turn counter
TURN_ID=$(cat "$TEMP_DIR/turn_id" 2>/dev/null || echo "0")
TURN_ID=$((TURN_ID + 1))
echo "$TURN_ID" > "$TEMP_DIR/turn_id"

# Reset tool counter for this turn
echo "0" > "$TEMP_DIR/tool_count"

exit 0
