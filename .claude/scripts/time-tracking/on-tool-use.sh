#!/bin/bash
# Hook: PostToolUse
# Purpose: Increment tool counter (runs frequently, must be fast!)
# Estimated execution: ~2-3ms

# Read JSON input from stdin (required but we only need session_id)
INPUT=$(cat)

# Extract session_id
SESSION_ID=$(echo "$INPUT" | grep -o '"session_id":"[^"]*"' | cut -d'"' -f4)

# Quick exit if no session
[ -z "$SESSION_ID" ] && exit 0

COUNTER_FILE="/tmp/claude-timing-${SESSION_ID}/tool_count"

# Quick exit if file doesn't exist
[ ! -f "$COUNTER_FILE" ] && exit 0

# Atomic increment
COUNT=$(cat "$COUNTER_FILE")
echo $((COUNT + 1)) > "$COUNTER_FILE"

exit 0
