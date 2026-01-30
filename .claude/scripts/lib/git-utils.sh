#!/bin/bash
#
# Shared utility functions for git and task operations
# Source this file in other scripts: source "$(dirname "$0")/lib/git-utils.sh"
#

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Status icons
STATUS_PENDING="⚪"
STATUS_IN_PROGRESS="🔵"
STATUS_COMPLETED="✅"

#######################################
# Get the repository root directory
# Outputs: Path to repo root
#######################################
get_repo_root() {
    git rev-parse --show-toplevel 2>/dev/null
}

#######################################
# Get current branch name
# Outputs: Current branch name
#######################################
get_current_branch() {
    git branch --show-current 2>/dev/null
}

#######################################
# Extract phase and task numbers from a branch name
# Arguments:
#   $1 - Branch name (optional, defaults to current branch)
# Outputs:
#   "XX YY" (phase task) or empty string
# Example:
#   get_phase_task_from_branch "phase-01/task-02-shared-kernel"
#   # Output: "01 02"
#######################################
get_phase_task_from_branch() {
    local branch="${1:-$(get_current_branch)}"

    # Match patterns like:
    # phase-01/task-02-description
    # phase-01/task-02
    # 01-02 (fallback)
    if [[ "$branch" =~ phase-([0-9]+)/task-([0-9]+) ]]; then
        printf "%s %s" "${BASH_REMATCH[1]}" "${BASH_REMATCH[2]}"
    elif [[ "$branch" =~ ^([0-9]{2})-([0-9]{2}) ]]; then
        printf "%s %s" "${BASH_REMATCH[1]}" "${BASH_REMATCH[2]}"
    fi
}

#######################################
# Find the phase directory for a given phase number
# Arguments:
#   $1 - Phase number (e.g., "01")
# Outputs:
#   Phase directory name (e.g., "phase-01-foundation")
#######################################
get_phase_directory() {
    local phase_num="$1"
    local project_dir="$(get_repo_root)/specification"

    # Find directory matching phase-XX-*
    local phase_dir=$(find "$project_dir" -maxdepth 1 -type d -name "phase-${phase_num}-*" 2>/dev/null | head -1)

    if [[ -n "$phase_dir" ]]; then
        basename "$phase_dir"
    fi
}

#######################################
# Get current phase from branch or detect first available
# Outputs:
#   Phase directory name
#######################################
get_current_phase() {
    local branch_info=$(get_phase_task_from_branch)
    local phase_num

    if [[ -n "$branch_info" ]]; then
        phase_num=$(echo "$branch_info" | cut -d' ' -f1)
    else
        # Default to phase-01
        phase_num="01"
    fi

    get_phase_directory "$phase_num"
}

#######################################
# Get the first active (not 100% completed) phase number
# This is useful when starting a new task without specifying phase
# Outputs:
#   Phase number (e.g., "03") or "01" if all completed
#######################################
get_active_phase_num() {
    local project_dir="$(get_repo_root)/specification"
    local phases=$(find "$project_dir" -maxdepth 1 -type d -name "phase-*" 2>/dev/null | sort)

    while IFS= read -r phase_path; do
        if [[ -z "$phase_path" ]]; then
            continue
        fi

        local phase_dir=$(basename "$phase_path")
        local tasks_dir="$phase_path/tasks"

        if [[ ! -d "$tasks_dir" ]]; then
            continue
        fi

        local task_files=$(find "$tasks_dir" -maxdepth 1 -type f -name "task-*.md" 2>/dev/null)
        if [[ -z "$task_files" ]]; then
            continue
        fi

        local total=0
        local completed=0

        while IFS= read -r task_file; do
            if [[ -z "$task_file" ]]; then
                continue
            fi
            ((total++))

            local metadata=$(parse_task_metadata "$task_file")
            local status_type=$(echo "$metadata" | jq -r '.status_type')

            if [[ "$status_type" == "completed" ]]; then
                ((completed++))
            fi
        done <<< "$task_files"

        # If this phase is not 100% completed, return its number
        if [[ $total -gt 0 && $completed -lt $total ]]; then
            echo "$phase_dir" | grep -o '[0-9]\+' | head -1
            return 0
        fi
    done <<< "$phases"

    # All phases completed, return first phase
    echo "01"
}

#######################################
# Get task file path for given phase and task numbers
# Arguments:
#   $1 - Phase number (e.g., "01")
#   $2 - Task number (e.g., "02")
# Outputs:
#   Full path to task file
#######################################
get_task_file_path() {
    local phase_num="$1"
    local task_num="$2"
    local project_dir="$(get_repo_root)/specification"
    local phase_dir=$(get_phase_directory "$phase_num")

    if [[ -z "$phase_dir" ]]; then
        return 1
    fi

    # Find task file matching task-XX-* or task-XX.md
    local task_file=$(find "$project_dir/$phase_dir/tasks" -maxdepth 1 -type f -name "task-${task_num}-*.md" 2>/dev/null | head -1)

    # Fallback to task-XX.md (without description suffix)
    if [[ -z "$task_file" ]]; then
        task_file=$(find "$project_dir/$phase_dir/tasks" -maxdepth 1 -type f -name "task-${task_num}.md" 2>/dev/null | head -1)
    fi

    if [[ -n "$task_file" ]]; then
        echo "$task_file"
    else
        return 1
    fi
}

#######################################
# Parse task metadata from markdown file
# Arguments:
#   $1 - Path to task file
# Outputs:
#   JSON object with id, status, dependencies, name
#######################################
parse_task_metadata() {
    local task_file="$1"

    if [[ ! -f "$task_file" ]]; then
        echo '{"error": "File not found"}'
        return 1
    fi

    local content=$(cat "$task_file")

    # Extract name from first heading
    local name=$(echo "$content" | grep -m1 "^# " | sed 's/^# //' | sed 's/Task [0-9]*: //')

    # Extract ID from metadata table, fallback to filename
    local id=$(echo "$content" | grep -i "| ID |" | sed 's/.*| ID |[[:space:]]*//' | sed 's/[[:space:]]*|.*//')

    # Fallback: extract ID from filename (task-01.md -> task-01)
    if [[ -z "$id" ]]; then
        id=$(basename "$task_file" .md)
    fi

    # Extract Status from metadata table
    local status=$(echo "$content" | grep -i "| Status |" | sed 's/.*| Status |[[:space:]]*//' | sed 's/[[:space:]]*|.*//')

    # Extract Dependencies from metadata table
    local deps_raw=$(echo "$content" | grep -i "| Dependencies |" | sed 's/.*| Dependencies |[[:space:]]*//' | sed 's/[[:space:]]*|.*//')

    # Convert dependencies to JSON array
    local deps_json="[]"
    if [[ -n "$deps_raw" && "$deps_raw" != "-" && "$deps_raw" != "none" ]]; then
        # Split by comma and create JSON array
        deps_json=$(echo "$deps_raw" | tr ',' '\n' | sed 's/^[[:space:]]*//' | sed 's/[[:space:]]*$//' | grep -v '^$' | jq -R . | jq -s .)
    fi

    # Determine status type (support both emoji and :emoji: formats)
    local status_type="pending"
    if [[ "$status" == *"$STATUS_COMPLETED"* || "$status" == *"completed"* || "$status" == *":white_check_mark:"* ]]; then
        status_type="completed"
    elif [[ "$status" == *"$STATUS_IN_PROGRESS"* || "$status" == *"in_progress"* || "$status" == *":large_blue_circle:"* ]]; then
        status_type="in_progress"
    fi
    # pending covers: ⚪, :white_circle:, or no match

    # Output JSON
    jq -n \
        --arg id "$id" \
        --arg name "$name" \
        --arg status "$status" \
        --arg status_type "$status_type" \
        --argjson deps "$deps_json" \
        --arg file "$task_file" \
        '{id: $id, name: $name, status: $status, status_type: $status_type, dependencies: $deps, file: $file}'
}

#######################################
# Update task status in markdown file
# Arguments:
#   $1 - Path to task file
#   $2 - New status (pending|in_progress|completed)
# Returns:
#   0 on success, 1 on failure
#######################################
update_task_status() {
    local task_file="$1"
    local new_status="$2"

    if [[ ! -f "$task_file" ]]; then
        echo "Error: Task file not found: $task_file" >&2
        return 1
    fi

    local status_icon
    case "$new_status" in
        pending)
            status_icon="$STATUS_PENDING pending"
            ;;
        in_progress)
            status_icon="$STATUS_IN_PROGRESS in_progress"
            ;;
        completed)
            status_icon="$STATUS_COMPLETED completed"
            ;;
        *)
            echo "Error: Invalid status: $new_status" >&2
            return 1
            ;;
    esac

    # Use sed to replace the status line in metadata table
    # Match: | Status | <anything> |
    # Replace with: | Status | <new_status> |
    if [[ "$(uname)" == "Darwin" ]]; then
        sed -i '' "s/| Status |[^|]*|/| Status | ${status_icon} |/" "$task_file"
    else
        sed -i "s/| Status |[^|]*|/| Status | ${status_icon} |/" "$task_file"
    fi
}

#######################################
# Check if all dependencies are completed
# Arguments:
#   $1 - Path to task file
# Returns:
#   0 if all dependencies completed, 1 if blocked
# Outputs:
#   List of blocking task IDs (one per line)
#######################################
check_dependencies_completed() {
    local task_file="$1"
    local metadata=$(parse_task_metadata "$task_file")
    local blocking_tasks=()

    # Get dependencies array
    local deps=$(echo "$metadata" | jq -r '.dependencies[]' 2>/dev/null)

    if [[ -z "$deps" ]]; then
        return 0
    fi

    local phase_num=$(echo "$metadata" | jq -r '.id' | grep -o '[0-9]\+' | head -1)
    # Extract phase from file path
    phase_num=$(echo "$task_file" | grep -o 'phase-[0-9]\+' | grep -o '[0-9]\+')

    while IFS= read -r dep_id; do
        if [[ -z "$dep_id" ]]; then
            continue
        fi

        local dep_task_num=$(echo "$dep_id" | grep -o '[0-9]\+')
        local dep_file=$(get_task_file_path "$phase_num" "$dep_task_num")

        if [[ -z "$dep_file" ]]; then
            # Dependency not found - warn but don't block
            echo "Warning: Dependency $dep_id not found" >&2
            continue
        fi

        local dep_metadata=$(parse_task_metadata "$dep_file")
        local dep_status=$(echo "$dep_metadata" | jq -r '.status_type')

        if [[ "$dep_status" != "completed" ]]; then
            blocking_tasks+=("$dep_id")
            echo "$dep_id"
        fi
    done <<< "$deps"

    if [[ ${#blocking_tasks[@]} -gt 0 ]]; then
        return 1
    fi

    return 0
}

#######################################
# Get task description (short) from branch name
# Arguments:
#   $1 - Branch name
# Outputs:
#   Description part of branch name
#######################################
get_task_description_from_branch() {
    local branch="$1"

    # Extract description from phase-XX/task-YY-description
    if [[ "$branch" =~ phase-[0-9]+/task-[0-9]+-(.+)$ ]]; then
        echo "${BASH_REMATCH[1]}"
    fi
}

#######################################
# Generate branch name from task file
# Arguments:
#   $1 - Path to task file
# Outputs:
#   Branch name (e.g., phase-01/task-02-shared-kernel)
#######################################
generate_branch_name() {
    local task_file="$1"

    # Extract phase number from path
    local phase_num=$(echo "$task_file" | grep -o 'phase-[0-9]\+' | grep -o '[0-9]\+')

    # Extract task filename without extension
    local task_filename=$(basename "$task_file" .md)

    echo "phase-${phase_num}/${task_filename}"
}

#######################################
# List all task files in a phase
# Arguments:
#   $1 - Phase directory name (e.g., "phase-01-foundation")
# Outputs:
#   List of task file paths
#######################################
list_phase_tasks() {
    local phase_dir="$1"
    local project_dir="$(get_repo_root)/specification"

    find "$project_dir/$phase_dir/tasks" -maxdepth 1 -type f -name "task-*.md" 2>/dev/null | sort
}

#######################################
# Check for uncommitted changes
# Returns:
#   0 if working directory is clean, 1 if dirty
#######################################
has_uncommitted_changes() {
    if [[ -n "$(git status --porcelain 2>/dev/null)" ]]; then
        return 0  # Has changes
    fi
    return 1  # Clean
}

#######################################
# Get main/master branch name
# Outputs:
#   "main" or "master"
#######################################
get_main_branch() {
    if git rev-parse --verify main >/dev/null 2>&1; then
        echo "main"
    elif git rev-parse --verify master >/dev/null 2>&1; then
        echo "master"
    else
        echo "main"  # Default
    fi
}

#######################################
# Print colored message
# Arguments:
#   $1 - Color (red|green|yellow|blue)
#   $2 - Message
#######################################
print_color() {
    local color="$1"
    local message="$2"

    case "$color" in
        red)    printf "${RED}%s${NC}\n" "$message" ;;
        green)  printf "${GREEN}%s${NC}\n" "$message" ;;
        yellow) printf "${YELLOW}%s${NC}\n" "$message" ;;
        blue)   printf "${BLUE}%s${NC}\n" "$message" ;;
        *)      printf "%s\n" "$message" ;;
    esac
}

#######################################
# Check if all tasks in a phase are completed
# Arguments:
#   $1 - Phase number (e.g., "01")
# Returns:
#   0 if all tasks completed, 1 otherwise
#######################################
all_phase_tasks_completed() {
    local phase_num="$1"
    local project_dir="$(get_repo_root)/specification"
    local phase_dir=$(get_phase_directory "$phase_num")

    if [[ -z "$phase_dir" ]]; then
        return 1
    fi

    local tasks_dir="$project_dir/$phase_dir/tasks"

    if [[ ! -d "$tasks_dir" ]]; then
        return 1
    fi

    local has_tasks=false
    for task_file in "$tasks_dir"/task-*.md; do
        [[ ! -f "$task_file" ]] && continue
        has_tasks=true

        local metadata=$(parse_task_metadata "$task_file")
        local status_type=$(echo "$metadata" | jq -r '.status_type')

        if [[ "$status_type" != "completed" ]]; then
            return 1
        fi
    done

    # Return success only if there were tasks
    if $has_tasks; then
        return 0
    fi
    return 1
}

#######################################
# Update phase metadata status in phase.md
# Arguments:
#   $1 - Path to phase.md file
#   $2 - New status (completed)
#######################################
update_phase_status() {
    local phase_file="$1"
    local new_status="$2"

    if [[ ! -f "$phase_file" ]]; then
        return 1
    fi

    if [[ "$new_status" == "completed" ]]; then
        if [[ "$(uname)" == "Darwin" ]]; then
            sed -i '' 's/| Status |[^|]*|/| Status | ✅ completed |/' "$phase_file"
        else
            sed -i 's/| Status |[^|]*|/| Status | ✅ completed |/' "$phase_file"
        fi
    fi
}

#######################################
# Try to check off scope items that match completed task names
# Arguments:
#   $1 - Path to phase.md file
#######################################
update_phase_scope_checklist() {
    local phase_file="$1"
    local phase_dir=$(dirname "$phase_file")
    local tasks_dir="$phase_dir/tasks"

    if [[ ! -d "$tasks_dir" ]]; then
        return 0
    fi

    # Get all completed task names and try to match them in scope
    for task_file in "$tasks_dir"/task-*.md; do
        [[ ! -f "$task_file" ]] && continue

        local metadata=$(parse_task_metadata "$task_file")
        local status_type=$(echo "$metadata" | jq -r '.status_type')

        # Only process completed tasks
        if [[ "$status_type" != "completed" ]]; then
            continue
        fi

        # Extract task name
        local task_name=$(echo "$metadata" | jq -r '.name')

        # Extract key words from task name (first 2-3 significant words)
        # Skip common words like "Add", "Create", "Implement"
        local keywords=$(echo "$task_name" | sed 's/\(Add\|Create\|Implement\|Setup\|Configure\|Define\) //gi' | head -c 30)

        if [[ -n "$keywords" ]]; then
            # Try to match and check off in scope (case insensitive partial match)
            # Convert "- [ ]" to "- [x]" for lines containing keywords
            if [[ "$(uname)" == "Darwin" ]]; then
                # Use perl for case-insensitive replacement on macOS
                perl -i -pe "s/^(- \[ \].*)/${1}/i if /\Q${keywords}\E/i; s/^- \[ \]/- [x]/ if /\Q${keywords}\E/i" "$phase_file" 2>/dev/null || true
            else
                sed -i "s/- \[ \]\(.*${keywords}.*\)/- [x]\1/i" "$phase_file" 2>/dev/null || true
            fi
        fi
    done
}
