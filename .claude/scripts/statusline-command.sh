#!/bin/bash

# Cache config
USAGE_CACHE="$HOME/.claude/usage-cache.json"
CACHE_MAX_AGE=300  # 5 minutes

# Fetch usage from API and save to cache (only if successful)
fetch_usage() {
    # macOS - get token from Keychain
    TOKEN=$(security find-generic-password -s "Claude Code-credentials" -w 2>/dev/null | jq -r '.claudeAiOauth.accessToken')

    # Linux - uncomment below, comment out macOS line above
    # TOKEN=$(cat ~/.claude/credentials.json | jq -r '.claudeAiOauth.accessToken')

    if [ -n "$TOKEN" ] && [ "$TOKEN" != "null" ]; then
        TEMP_FILE=$(mktemp)
        curl -s "https://api.anthropic.com/api/oauth/usage" \
            -H "Authorization: Bearer $TOKEN" \
            -H "anthropic-beta: oauth-2025-04-20" > "$TEMP_FILE" 2>/dev/null

        # Only save to cache if response is valid (has five_hour field, not an error)
        if jq -e '.five_hour' "$TEMP_FILE" > /dev/null 2>&1; then
            mv "$TEMP_FILE" "$USAGE_CACHE"
        else
            rm -f "$TEMP_FILE"
        fi
    fi
}

# Check cache and refresh if stale
if [ -f "$USAGE_CACHE" ]; then
    cache_age=$(($(date +%s) - $(stat -f %m "$USAGE_CACHE")))
    if [ "$cache_age" -gt "$CACHE_MAX_AGE" ]; then
        # Refresh in background (non-blocking)
        (fetch_usage &) 2>/dev/null
    fi
else
    # No cache, fetch in background
    (fetch_usage &) 2>/dev/null
fi

# Read JSON input from stdin
input=$(cat)

# Debug: log input to file
echo "$input" > /tmp/statusline-debug.json

# Extract values from JSON
cwd=$(echo "$input" | jq -r '.workspace.current_dir')
model_name=$(echo "$input" | jq -r '.model.display_name')

# Get context percentage directly from the provided field
context_percent=$(echo "$input" | jq -r '.context_window.used_percentage // 0 | floor')

# Get current directory name (basename)
dir_name=$(basename "$cwd")

# Get git branch
git_part=""
if git -C "$cwd" rev-parse --git-dir > /dev/null 2>&1; then
    branch=$(git -C "$cwd" -c core.useBuiltinFSMonitor=false rev-parse --abbrev-ref HEAD 2>/dev/null)
    if [ -n "$branch" ]; then
        if [ -n "$(git -C "$cwd" -c core.useBuiltinFSMonitor=false status --porcelain 2>/dev/null)" ]; then
            git_part=$(printf " | \033[2m%s*\033[0m" "$branch")
        else
            git_part=$(printf " | \033[2m%s\033[0m" "$branch")
        fi
    fi
fi

# Context color (only highlight when > 70%)
if [ "$context_percent" -gt 70 ]; then
    ctx_color="2;31"  # dim red
else
    ctx_color="2"     # just dim
fi

# Parse usage data from cache
session_part=""
weekly_part=""
if [ -f "$USAGE_CACHE" ]; then
    # Session (5-hour) usage
    session_pct=$(jq -r '.five_hour.utilization // 0 | floor' "$USAGE_CACHE" 2>/dev/null)
    session_reset_iso=$(jq -r '.five_hour.resets_at // ""' "$USAGE_CACHE" 2>/dev/null)

    # Weekly (7-day) usage
    weekly_pct=$(jq -r '.seven_day.utilization // 0 | floor' "$USAGE_CACHE" 2>/dev/null)

    # Format session reset time (extract HH:MM in local time)
    reset_time="--"
    if [ -n "$session_reset_iso" ] && [ "$session_reset_iso" != "null" ]; then
        # Convert UTC ISO to local time HH:MM (works for any timezone)
        # Strip timezone suffix and fractional seconds
        clean_iso="${session_reset_iso%%+*}"
        clean_iso="${clean_iso%%.*}"
        # Parse as UTC → epoch → local time (universal approach)
        if command -v gdate &> /dev/null; then
            # Linux (GNU date) or macOS with coreutils
            reset_time=$(TZ=UTC gdate -d "$clean_iso" "+%s" 2>/dev/null | xargs -I{} date -d "@{}" "+%H:%M" 2>/dev/null || echo "--")
        else
            # macOS (BSD date)
            epoch=$(TZ=UTC date -j -f "%Y-%m-%dT%H:%M:%S" "$clean_iso" "+%s" 2>/dev/null)
            [ -n "$epoch" ] && reset_time=$(date -j -f "%s" "$epoch" "+%H:%M" 2>/dev/null || echo "--")
        fi
    fi

    # Session color: yellow when > 80%
    if [ "$session_pct" -gt 80 ]; then
        session_color="33"  # yellow
    else
        session_color="2"   # dim
    fi

    # Weekly color: yellow when > 90%
    if [ "$weekly_pct" -gt 90 ]; then
        weekly_color="33"  # yellow
    else
        weekly_color="2"   # dim
    fi

    session_part=$(printf " \033[2m|\033[0m \033[%sm%s%%@%s\033[0m" "$session_color" "$session_pct" "$reset_time")
    weekly_part=$(printf " \033[2m|\033[0m \033[%smwk:%s%%\033[0m" "$weekly_color" "$weekly_pct")
else
    session_part=$(printf " \033[2m|\033[0m \033[2m--\033[0m")
    weekly_part=$(printf " \033[2m|\033[0m \033[2mwk:--\033[0m")
fi

# Build output: dir | branch | model | context% | session%@time | wk:weekly%
printf "\033[2m%s\033[0m%s \033[2m|\033[0m \033[2m%s\033[0m \033[2m|\033[0m \033[%sm%s%%\033[0m%s%s" "$dir_name" "$git_part" "$model_name" "$ctx_color" "$context_percent" "$session_part" "$weekly_part"
