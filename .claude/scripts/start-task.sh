#!/bin/bash
#
# Start Task Script
# Validates dependencies, creates branch, and updates task status
#
# Usage:
#   start-task.sh 02                              # Task 02 in current phase
#   start-task.sh task-02                         # Explicit task-02
#   start-task.sh phase-01/task-02-shared-kernel  # Full branch name
#   start-task.sh --json                          # JSON output
#

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/lib/git-utils.sh"

# Parse arguments
JSON_OUTPUT=false
TASK_INPUT=""

for arg in "$@"; do
    case "$arg" in
        --json)
            JSON_OUTPUT=true
            ;;
        *)
            if [[ -z "$TASK_INPUT" ]]; then
                TASK_INPUT="$arg"
            fi
            ;;
    esac
done

if [[ -z "$TASK_INPUT" ]]; then
    echo "Error: Task identifier required"
    echo "Usage: start-task.sh <task-number|task-XX|phase-XX/task-YY-description>"
    exit 1
fi

REPO_ROOT=$(get_repo_root)
PROJECT_DIR="$REPO_ROOT/.claude/project"

#######################################
# Output result in appropriate format
# Arguments:
#   $1 - Status (success|error)
#   $2 - Message
#   $3 - Branch name (optional)
#   $4 - Task file (optional)
#   $5 - Warnings JSON array (optional)
#######################################
output_result() {
    local status="$1"
    local message="$2"
    local branch="${3:-}"
    local task_file="${4:-}"
    local warnings="${5:-[]}"

    if $JSON_OUTPUT; then
        jq -n \
            --arg status "$status" \
            --arg message "$message" \
            --arg branch "$branch" \
            --arg task_file "$task_file" \
            --argjson warnings "$warnings" \
            '{status: $status, message: $message, branch: $branch, task_file: $task_file, warnings: $warnings}'
    else
        if [[ "$status" == "success" ]]; then
            print_color green "$message"
            [[ -n "$branch" ]] && echo "Branch: $branch"
            [[ -n "$task_file" ]] && echo "Task file: $task_file"
        else
            print_color red "Error: $message"
        fi
    fi
}

# Parse task input
PHASE_NUM=""
TASK_NUM=""
BRANCH_NAME=""

if [[ "$TASK_INPUT" =~ ^phase-([0-9]+)/task-([0-9]+) ]]; then
    # Full branch name: phase-01/task-02-shared-kernel
    PHASE_NUM="${BASH_REMATCH[1]}"
    TASK_NUM="${BASH_REMATCH[2]}"
    BRANCH_NAME="$TASK_INPUT"
elif [[ "$TASK_INPUT" =~ ^phase-([0-9]+)/([0-9]+)$ ]]; then
    # Shorthand: phase-02/01
    PHASE_NUM="${BASH_REMATCH[1]}"
    TASK_NUM=$(printf "%02d" "${BASH_REMATCH[2]}")
elif [[ "$TASK_INPUT" =~ ^([0-9]+)/([0-9]+)$ ]]; then
    # Even shorter: 02/01
    PHASE_NUM=$(printf "%02d" "${BASH_REMATCH[1]}")
    TASK_NUM=$(printf "%02d" "${BASH_REMATCH[2]}")
elif [[ "$TASK_INPUT" =~ ^([0-9]+)-([0-9]+)$ ]]; then
    # Dash format: 02-01
    PHASE_NUM=$(printf "%02d" "${BASH_REMATCH[1]}")
    TASK_NUM=$(printf "%02d" "${BASH_REMATCH[2]}")
elif [[ "$TASK_INPUT" =~ ^task-([0-9]+) ]]; then
    # task-02 format
    TASK_NUM="${BASH_REMATCH[1]}"
elif [[ "$TASK_INPUT" =~ ^([0-9]+)$ ]]; then
    # Just number: 02
    TASK_NUM=$(printf "%02d" "$TASK_INPUT")
else
    output_result "error" "Invalid task identifier: $TASK_INPUT"
    exit 1
fi

# Detect phase if not specified
if [[ -z "$PHASE_NUM" ]]; then
    # Try to get from current branch
    CURRENT_PHASE_TASK=$(get_phase_task_from_branch)
    if [[ -n "$CURRENT_PHASE_TASK" ]]; then
        PHASE_NUM=$(echo "$CURRENT_PHASE_TASK" | cut -d' ' -f1)
    else
        # Find first active (not 100% completed) phase
        PHASE_NUM=$(get_active_phase_num)
    fi
fi

# Find task file
TASK_FILE=$(get_task_file_path "$PHASE_NUM" "$TASK_NUM")

if [[ -z "$TASK_FILE" || ! -f "$TASK_FILE" ]]; then
    output_result "error" "Task file not found for phase-$PHASE_NUM/task-$TASK_NUM"
    exit 1
fi

# Parse task metadata
TASK_METADATA=$(parse_task_metadata "$TASK_FILE")
TASK_ID=$(echo "$TASK_METADATA" | jq -r '.id')
TASK_NAME=$(echo "$TASK_METADATA" | jq -r '.name')
TASK_STATUS=$(echo "$TASK_METADATA" | jq -r '.status_type')

# Check if already in progress or completed
if [[ "$TASK_STATUS" == "completed" ]]; then
    output_result "error" "Task $TASK_ID is already completed"
    exit 1
fi

if [[ "$TASK_STATUS" == "in_progress" ]]; then
    output_result "error" "Task $TASK_ID is already in progress"
    exit 1
fi

# Check dependencies
WARNINGS='[]'
BLOCKING_TASKS=""
if ! BLOCKING_TASKS=$(check_dependencies_completed "$TASK_FILE" 2>&1); then
    if [[ -n "$BLOCKING_TASKS" ]]; then
        output_result "error" "Task $TASK_ID is blocked by: $BLOCKING_TASKS"
        exit 1
    fi
fi

# Generate branch name if not provided
if [[ -z "$BRANCH_NAME" ]]; then
    BRANCH_NAME=$(generate_branch_name "$TASK_FILE")
fi

# Check for uncommitted changes
if has_uncommitted_changes; then
    WARNINGS=$(echo "$WARNINGS" | jq '. + ["Uncommitted changes in working directory"]')
    if ! $JSON_OUTPUT; then
        print_color yellow "Warning: You have uncommitted changes in your working directory"
    fi
fi

# Check if branch already exists
MAIN_BRANCH=$(get_main_branch)
if git rev-parse --verify "$BRANCH_NAME" >/dev/null 2>&1; then
    # Branch exists, just checkout
    git checkout "$BRANCH_NAME" >/dev/null 2>&1
    if ! $JSON_OUTPUT; then
        print_color yellow "Switched to existing branch: $BRANCH_NAME"
    fi
else
    # Create new branch from main
    git checkout -b "$BRANCH_NAME" "$MAIN_BRANCH" >/dev/null 2>&1
    if ! $JSON_OUTPUT; then
        print_color green "Created and switched to branch: $BRANCH_NAME"
    fi
fi

# Update task status to in_progress
update_task_status "$TASK_FILE" "in_progress"

# Output success
if $JSON_OUTPUT; then
    jq -n \
        --arg status "success" \
        --arg message "Started task $TASK_ID: $TASK_NAME" \
        --arg branch "$BRANCH_NAME" \
        --arg task_file "$TASK_FILE" \
        --argjson warnings "$WARNINGS" \
        '{status: $status, message: $message, branch: $branch, task_file: $task_file, warnings: $warnings}'
else
    echo ""
    print_color green "Started task $TASK_ID: $TASK_NAME"
    echo "Branch: $BRANCH_NAME"
    echo "Task file: $TASK_FILE"
    echo ""
    echo "Task scope:"
    # Show scope section from task file (macOS compatible)
    awk '/^## Scope/{found=1; next} /^## /{found=0} found' "$TASK_FILE"
fi
