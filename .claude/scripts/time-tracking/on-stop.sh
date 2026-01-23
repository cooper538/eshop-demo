#!/bin/bash
# Hook: Stop
# Purpose: Calculate duration, write CSV row
# Estimated execution: ~5-8ms

set -e

# Read JSON input from stdin
INPUT=$(cat)

# Extract session_id
SESSION_ID=$(echo "$INPUT" | grep -o '"session_id":"[^"]*"' | cut -d'"' -f4)

# Quick exit if no session
if [ -z "$SESSION_ID" ]; then
    exit 0
fi

TEMP_DIR="/tmp/claude-timing-${SESSION_ID}"

# Quick exit if temp dir doesn't exist (no tracking data)
if [ ! -d "$TEMP_DIR" ]; then
    exit 0
fi

# Quick exit if no start time (UserPromptSubmit wasn't triggered)
if [ ! -f "$TEMP_DIR/start_time" ]; then
    exit 0
fi

# Get project directory (where logs will be stored)
PROJECT_DIR="${CLAUDE_PROJECT_DIR:-$(pwd)}"
LOG_DIR="$PROJECT_DIR/.claude/logs"
LOG_FILE="$LOG_DIR/time-tracking.csv"

# Ensure log directory exists
mkdir -p "$LOG_DIR"

# Create header if file doesn't exist
if [ ! -f "$LOG_FILE" ]; then
    echo "date,branch,phase,task,session_id,turn_id,start_iso,end_iso,duration_sec,base_commit,tool_calls" > "$LOG_FILE"
fi

# Read cached values
START_TIME=$(cat "$TEMP_DIR/start_time")
START_EPOCH=$(cat "$TEMP_DIR/start_epoch" 2>/dev/null || echo "0")
BRANCH=$(cat "$TEMP_DIR/branch" 2>/dev/null || echo "no-branch")
BASE_COMMIT=$(cat "$TEMP_DIR/base_commit" 2>/dev/null || echo "no-commit")
TURN_ID=$(cat "$TEMP_DIR/turn_id" 2>/dev/null || echo "0")
TOOL_COUNT=$(cat "$TEMP_DIR/tool_count" 2>/dev/null || echo "0")

# Calculate end time
END_TIME=$(date +%Y-%m-%dT%H:%M:%S.%3N)
END_EPOCH=$(date +%s.%N)

# Calculate duration in seconds (with decimals)
if [ "$START_EPOCH" != "0" ]; then
    DURATION=$(echo "$END_EPOCH - $START_EPOCH" | bc -l 2>/dev/null || echo "0")
    # Round to 3 decimal places
    DURATION=$(printf "%.3f" "$DURATION")
else
    DURATION="0.000"
fi

# Extract date
DATE=$(date +%Y-%m-%d)

# Parse phase and task from branch name (format: phase-XX/task-YY-description)
PHASE=""
TASK=""
if [[ "$BRANCH" =~ ^phase-([0-9]+)/task-([0-9]+) ]]; then
    PHASE="${BASH_REMATCH[1]}"
    TASK="${BASH_REMATCH[2]}"
fi

# Write CSV row
echo "${DATE},${BRANCH},${PHASE},${TASK},${SESSION_ID},${TURN_ID},${START_TIME},${END_TIME},${DURATION},${BASE_COMMIT},${TOOL_COUNT}" >> "$LOG_FILE"

# Clear start time to prevent duplicate entries (but keep other data for next turn)
rm -f "$TEMP_DIR/start_time" "$TEMP_DIR/start_epoch"

exit 0
