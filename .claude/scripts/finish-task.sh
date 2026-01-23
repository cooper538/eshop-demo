#!/bin/bash
#
# Finish Task Script
# Runs tests, updates status, and merges to main
#
# Usage:
#   finish-task.sh              # Finish current task
#   finish-task.sh --no-merge   # Without merge to main
#   finish-task.sh --no-test    # Without running tests
#   finish-task.sh --json       # JSON output
#

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/lib/git-utils.sh"

# Parse arguments
JSON_OUTPUT=false
NO_MERGE=false
NO_TEST=false
FORCE=false

for arg in "$@"; do
    case "$arg" in
        --json)
            JSON_OUTPUT=true
            ;;
        --no-merge)
            NO_MERGE=true
            ;;
        --no-test)
            NO_TEST=true
            ;;
        --force)
            FORCE=true
            ;;
    esac
done

REPO_ROOT=$(get_repo_root)
PROJECT_DIR="$REPO_ROOT/.claude/project"

#######################################
# Output result in appropriate format
#######################################
output_result() {
    local status="$1"
    local message="$2"
    local details="${3:-}"

    if $JSON_OUTPUT; then
        jq -n \
            --arg status "$status" \
            --arg message "$message" \
            --arg details "$details" \
            '{status: $status, message: $message, details: $details}'
    else
        if [[ "$status" == "success" ]]; then
            print_color green "$message"
        elif [[ "$status" == "warning" ]]; then
            print_color yellow "$message"
        else
            print_color red "Error: $message"
        fi
        [[ -n "$details" ]] && echo "$details"
    fi
}

# Get current branch info
CURRENT_BRANCH=$(get_current_branch)
PHASE_TASK=$(get_phase_task_from_branch "$CURRENT_BRANCH")

if [[ -z "$PHASE_TASK" ]]; then
    output_result "error" "Not on a task branch. Expected format: phase-XX/task-YY-description"
    exit 1
fi

PHASE_NUM=$(echo "$PHASE_TASK" | cut -d' ' -f1)
TASK_NUM=$(echo "$PHASE_TASK" | cut -d' ' -f2)

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

if ! $JSON_OUTPUT; then
    echo ""
    print_color blue "Finishing task: $TASK_ID - $TASK_NAME"
    echo "Branch: $CURRENT_BRANCH"
    echo ""
fi

# Check for uncommitted changes
if has_uncommitted_changes; then
    if ! $FORCE; then
        output_result "error" "You have uncommitted changes. Commit or stash them first, or use --force"
        exit 1
    else
        output_result "warning" "Uncommitted changes present (continuing due to --force)"
    fi
fi

# Run tests (unless --no-test)
if ! $NO_TEST; then
    if ! $JSON_OUTPUT; then
        print_color blue "Running unit tests..."
    fi

    # Check if solution file exists
    SOLUTION_FILE="$REPO_ROOT/src/EShopDemo.sln"
    if [[ -f "$SOLUTION_FILE" ]]; then
        if ! dotnet test "$SOLUTION_FILE" --filter "Category!=Integration" --no-build 2>/dev/null; then
            # Try with build
            if ! dotnet test "$SOLUTION_FILE" --filter "Category!=Integration" 2>&1; then
                output_result "error" "Unit tests failed"
                exit 1
            fi
        fi

        if ! $JSON_OUTPUT; then
            print_color green "Unit tests passed"
        fi

        # Integration tests (commented out as per plan)
        # if ! $JSON_OUTPUT; then
        #     print_color blue "Running integration tests..."
        # fi
        # dotnet test "$SOLUTION_FILE" --filter "Category=Integration"
    else
        if ! $JSON_OUTPUT; then
            print_color yellow "No solution file found, skipping tests"
        fi
    fi
fi

# Run CSharpier check
if ! $JSON_OUTPUT; then
    print_color blue "Checking code formatting..."
fi

if command -v dotnet &> /dev/null; then
    if ! dotnet csharpier . --check 2>/dev/null; then
        output_result "error" "CSharpier check failed. Run 'dotnet csharpier .' to fix"
        exit 1
    fi
    if ! $JSON_OUTPUT; then
        print_color green "Code formatting OK"
    fi
fi

# Update task status to completed
update_task_status "$TASK_FILE" "completed"
if ! $JSON_OUTPUT; then
    print_color green "Task status updated to completed"
fi

# Commit the status change
if has_uncommitted_changes; then
    git add "$TASK_FILE"
    git commit -m "[$PHASE_NUM-$TASK_NUM] docs: mark task as completed" --no-verify 2>/dev/null || true
fi

# Merge to main (unless --no-merge)
MAIN_BRANCH=$(get_main_branch)

if ! $NO_MERGE; then
    if ! $JSON_OUTPUT; then
        print_color blue "Merging to $MAIN_BRANCH..."
    fi

    # Checkout main
    git checkout "$MAIN_BRANCH" >/dev/null 2>&1

    # Merge the task branch
    if ! git merge "$CURRENT_BRANCH" --no-ff -m "Merge branch '$CURRENT_BRANCH'" 2>&1; then
        output_result "error" "Merge failed. Resolve conflicts manually"
        exit 1
    fi

    if ! $JSON_OUTPUT; then
        print_color green "Merged $CURRENT_BRANCH into $MAIN_BRANCH"
    fi

    # Delete the task branch
    git branch -d "$CURRENT_BRANCH" >/dev/null 2>&1 || true

    if ! $JSON_OUTPUT; then
        print_color green "Deleted branch: $CURRENT_BRANCH"
    fi
else
    if ! $JSON_OUTPUT; then
        print_color yellow "Skipping merge (--no-merge specified)"
        echo "To merge manually:"
        echo "  git checkout $MAIN_BRANCH && git merge $CURRENT_BRANCH"
    fi
fi

# Final output
if $JSON_OUTPUT; then
    jq -n \
        --arg status "success" \
        --arg message "Task $TASK_ID completed successfully" \
        --arg task_id "$TASK_ID" \
        --arg task_name "$TASK_NAME" \
        --arg branch "$CURRENT_BRANCH" \
        --argjson merged "$(if $NO_MERGE; then echo false; else echo true; fi)" \
        '{status: $status, message: $message, task_id: $task_id, task_name: $task_name, branch: $branch, merged: $merged}'
else
    echo ""
    print_color green "Task $TASK_ID completed successfully!"
    echo ""
    echo "Next steps:"
    echo "  - Run /task-status to see what's next"
    echo "  - Run /start-task <number> to start the next task"
fi
