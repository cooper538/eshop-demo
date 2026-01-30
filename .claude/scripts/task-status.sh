#!/bin/bash
#
# Task Status Script
# Shows overview of task status with visual indicators
#
# Usage:
#   task-status.sh              # Current phase (or all if not in branch)
#   task-status.sh phase-02     # Specific phase
#   task-status.sh --all        # All phases
#   task-status.sh --json       # JSON output
#

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/lib/git-utils.sh"

# Parse arguments
SHOW_ALL=false
JSON_OUTPUT=false
SPECIFIC_PHASE=""

for arg in "$@"; do
    case "$arg" in
        --all)
            SHOW_ALL=true
            ;;
        --json)
            JSON_OUTPUT=true
            ;;
        phase-*)
            SPECIFIC_PHASE="$arg"
            ;;
        *)
            # Try to match just a number like "01"
            if [[ "$arg" =~ ^[0-9]+$ ]]; then
                SPECIFIC_PHASE="phase-$(printf "%02d" "$arg")"
            fi
            ;;
    esac
done

REPO_ROOT=$(get_repo_root)
PROJECT_DIR="$REPO_ROOT/specification"

# Get current branch info for "YOU ARE HERE" marker
CURRENT_BRANCH=$(get_current_branch)
CURRENT_PHASE_TASK=$(get_phase_task_from_branch "$CURRENT_BRANCH")
CURRENT_PHASE=""
CURRENT_TASK=""
if [[ -n "$CURRENT_PHASE_TASK" ]]; then
    CURRENT_PHASE=$(echo "$CURRENT_PHASE_TASK" | cut -d' ' -f1)
    CURRENT_TASK=$(echo "$CURRENT_PHASE_TASK" | cut -d' ' -f2)
fi

#######################################
# Process a single phase
# Arguments:
#   $1 - Phase directory name
# Outputs:
#   Formatted status or JSON
#######################################
process_phase() {
    local phase_dir="$1"
    local phase_path="$PROJECT_DIR/$phase_dir"
    local tasks_dir="$phase_path/tasks"

    if [[ ! -d "$tasks_dir" ]]; then
        return
    fi

    local phase_num=$(echo "$phase_dir" | grep -o '[0-9]\+')
    # Extract name part (after phase-XX-)
    local phase_name_raw=$(echo "$phase_dir" | sed 's/phase-[0-9]*-//')
    # Convert kebab-case to Title Case
    local phase_name=$(echo "$phase_name_raw" | tr '-' ' ' | awk '{for(i=1;i<=NF;i++) $i=toupper(substr($i,1,1)) tolower(substr($i,2))}1')

    local task_files=$(find "$tasks_dir" -maxdepth 1 -type f -name "task-*.md" 2>/dev/null | sort)

    if [[ -z "$task_files" ]]; then
        return
    fi

    local total_tasks=0
    local completed_tasks=0
    local in_progress_tasks=0
    local tasks_data=()

    while IFS= read -r task_file; do
        if [[ -z "$task_file" ]]; then
            continue
        fi

        local metadata=$(parse_task_metadata "$task_file")
        local task_id=$(echo "$metadata" | jq -r '.id')
        local task_name=$(echo "$metadata" | jq -r '.name')
        local status_type=$(echo "$metadata" | jq -r '.status_type')
        local deps=$(echo "$metadata" | jq -r '.dependencies | join(", ")')

        ((total_tasks++))

        case "$status_type" in
            completed)
                ((completed_tasks++))
                ;;
            in_progress)
                ((in_progress_tasks++))
                ;;
        esac

        # Check if this is current task
        local task_num=$(echo "$task_id" | grep -o '[0-9]\+')
        local is_current=false
        if [[ "$phase_num" == "$CURRENT_PHASE" && "$task_num" == "$CURRENT_TASK" ]]; then
            is_current=true
        fi

        # Check if blocked
        local is_blocked=false
        local blocking_tasks=""
        if [[ "$status_type" == "pending" && -n "$deps" && "$deps" != "null" ]]; then
            # Check each dependency
            IFS=',' read -ra dep_array <<< "$deps"
            for dep in "${dep_array[@]}"; do
                dep=$(echo "$dep" | tr -d ' ')
                local dep_num=$(echo "$dep" | grep -o '[0-9]\+')
                local dep_file=$(get_task_file_path "$phase_num" "$dep_num" 2>/dev/null)
                if [[ -n "$dep_file" ]]; then
                    local dep_meta=$(parse_task_metadata "$dep_file")
                    local dep_status=$(echo "$dep_meta" | jq -r '.status_type')
                    if [[ "$dep_status" != "completed" ]]; then
                        is_blocked=true
                        if [[ -n "$blocking_tasks" ]]; then
                            blocking_tasks="$blocking_tasks, $dep"
                        else
                            blocking_tasks="$dep"
                        fi
                    fi
                fi
            done
        fi

        tasks_data+=("$(jq -n \
            --arg id "$task_id" \
            --arg name "$task_name" \
            --arg status "$status_type" \
            --arg deps "$deps" \
            --argjson is_current "$is_current" \
            --argjson is_blocked "$is_blocked" \
            --arg blocking "$blocking_tasks" \
            '{id: $id, name: $name, status: $status, dependencies: $deps, is_current: $is_current, is_blocked: $is_blocked, blocking_tasks: $blocking}'
        )")

    done <<< "$task_files"

    if $JSON_OUTPUT; then
        # Output JSON for this phase
        local tasks_json=$(printf '%s\n' "${tasks_data[@]}" | jq -s '.')
        jq -n \
            --arg phase "$phase_dir" \
            --arg phase_name "$phase_name" \
            --argjson total "$total_tasks" \
            --argjson completed "$completed_tasks" \
            --argjson in_progress "$in_progress_tasks" \
            --argjson tasks "$tasks_json" \
            '{phase: $phase, name: $phase_name, total: $total, completed: $completed, in_progress: $in_progress, tasks: $tasks}'
    else
        # Human-readable output
        echo ""
        printf "Phase %s: %s\n" "$phase_num" "$phase_name"
        printf '%.0s━' {1..50}
        echo ""

        for task_json in "${tasks_data[@]}"; do
            local task_id=$(echo "$task_json" | jq -r '.id')
            local task_name=$(echo "$task_json" | jq -r '.name')
            local status=$(echo "$task_json" | jq -r '.status')
            local is_current=$(echo "$task_json" | jq -r '.is_current')
            local is_blocked=$(echo "$task_json" | jq -r '.is_blocked')
            local blocking=$(echo "$task_json" | jq -r '.blocking_tasks')

            # Status icon
            local icon="$STATUS_PENDING"
            case "$status" in
                completed)
                    icon="$STATUS_COMPLETED"
                    ;;
                in_progress)
                    icon="$STATUS_IN_PROGRESS"
                    ;;
            esac

            # Build line
            local line="$icon $task_id $task_name"

            # Add markers
            local suffix=""
            if [[ "$is_current" == "true" ]]; then
                suffix="                    ← YOU ARE HERE"
            elif [[ "$is_blocked" == "true" && -n "$blocking" ]]; then
                suffix=" (blocked by: $blocking)"
            fi

            printf "%s%s\n" "$line" "$suffix"
        done

        # Progress
        local progress_pct=0
        if [[ $total_tasks -gt 0 ]]; then
            progress_pct=$((completed_tasks * 100 / total_tasks))
        fi
        echo ""
        printf "Progress: %d/%d (%d%%)\n" "$completed_tasks" "$total_tasks" "$progress_pct"

        # Next available task
        for task_json in "${tasks_data[@]}"; do
            local status=$(echo "$task_json" | jq -r '.status')
            local is_blocked=$(echo "$task_json" | jq -r '.is_blocked')
            local task_id=$(echo "$task_json" | jq -r '.id')

            if [[ "$status" == "in_progress" ]]; then
                printf "Next available: %s (in progress)\n" "$task_id"
                break
            elif [[ "$status" == "pending" && "$is_blocked" != "true" ]]; then
                printf "Next available: %s\n" "$task_id"
                break
            fi
        done
    fi
}

# Find phases to process
if $SHOW_ALL; then
    PHASES=$(find "$PROJECT_DIR" -maxdepth 1 -type d -name "phase-*" 2>/dev/null | sort)
elif [[ -n "$SPECIFIC_PHASE" ]]; then
    # Find matching phase directory
    PHASES=$(find "$PROJECT_DIR" -maxdepth 1 -type d -name "${SPECIFIC_PHASE}*" 2>/dev/null | head -1)
else
    # Detect from current branch or default to first phase
    if [[ -n "$CURRENT_PHASE" ]]; then
        PHASES=$(find "$PROJECT_DIR" -maxdepth 1 -type d -name "phase-${CURRENT_PHASE}-*" 2>/dev/null | head -1)
    fi
    # Fallback to all phases if not in a task branch
    if [[ -z "$PHASES" ]]; then
        PHASES=$(find "$PROJECT_DIR" -maxdepth 1 -type d -name "phase-*" 2>/dev/null | sort)
    fi
fi

if [[ -z "$PHASES" ]]; then
    echo "No phase directories found in $PROJECT_DIR"
    exit 1
fi

if $JSON_OUTPUT; then
    # Collect all phases and output as JSON array
    phases_output=()
    while IFS= read -r phase_path; do
        if [[ -z "$phase_path" ]]; then
            continue
        fi
        phase_dir=$(basename "$phase_path")
        phase_json=$(process_phase "$phase_dir")
        if [[ -n "$phase_json" ]]; then
            phases_output+=("$phase_json")
        fi
    done <<< "$PHASES"

    printf '%s\n' "${phases_output[@]}" | jq -s '.'
else
    while IFS= read -r phase_path; do
        if [[ -z "$phase_path" ]]; then
            continue
        fi
        phase_dir=$(basename "$phase_path")
        process_phase "$phase_dir"
    done <<< "$PHASES"
fi
