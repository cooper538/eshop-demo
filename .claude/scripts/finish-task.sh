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
PROJECT_DIR="$REPO_ROOT/specification"

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

# Detect mode: MAIN, FEATURE_BRANCH, or WORKTREE
CURRENT_BRANCH=$(get_current_branch)
MAIN_BRANCH=$(get_main_branch)
IS_WORKTREE=false
MODE="MAIN"

# Check if we're in a worktree
if [[ -f "$(get_repo_root)/.git" ]]; then
    IS_WORKTREE=true
    MODE="WORKTREE"
elif [[ "$CURRENT_BRANCH" != "$MAIN_BRANCH" ]]; then
    MODE="FEATURE_BRANCH"
fi

# Extract phase/task from branch name (for FEATURE_BRANCH/WORKTREE)
PHASE_TASK=$(get_phase_task_from_branch "$CURRENT_BRANCH")
PHASE_NUM=""
TASK_NUM=""

if [[ -n "$PHASE_TASK" ]]; then
    PHASE_NUM=$(echo "$PHASE_TASK" | cut -d' ' -f1)
    TASK_NUM=$(echo "$PHASE_TASK" | cut -d' ' -f2)
fi

# For MAIN mode, find task from in_progress status
if [[ "$MODE" == "MAIN" && -z "$PHASE_NUM" ]]; then
    # Find in_progress task
    PROJECT_DIR="$(get_repo_root)/specification"
    for phase_path in "$PROJECT_DIR"/phase-*/; do
        if [[ ! -d "$phase_path/tasks" ]]; then
            continue
        fi
        for task_file in "$phase_path/tasks"/task-*.md; do
            [[ ! -f "$task_file" ]] && continue
            local_meta=$(parse_task_metadata "$task_file")
            local_status=$(echo "$local_meta" | jq -r '.status_type')
            if [[ "$local_status" == "in_progress" ]]; then
                PHASE_NUM=$(echo "$task_file" | grep -o 'phase-[0-9]\+' | grep -o '[0-9]\+')
                TASK_NUM=$(basename "$task_file" .md | grep -o '[0-9]\+' | head -1)
                break 2
            fi
        done
    done
fi

if [[ -z "$PHASE_NUM" || -z "$TASK_NUM" ]]; then
    output_result "error" "Could not determine current task. Either be on a task branch or have a task with 🔵 in_progress status."
    exit 1
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

if ! $JSON_OUTPUT; then
    echo ""
    print_color blue "Finishing task: $TASK_ID - $TASK_NAME"
    echo "Mode: $MODE"
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
    if ! dotnet csharpier check . 2>/dev/null; then
        output_result "error" "CSharpier check failed. Run 'dotnet csharpier format .' to fix"
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

# ============================================
# PHASE COMPLETION CHECK
# ============================================

# Check if all tasks in phase are now completed
if all_phase_tasks_completed "$PHASE_NUM"; then
    PHASE_DIR=$(get_phase_directory "$PHASE_NUM")
    PHASE_FILE="$PROJECT_DIR/$PHASE_DIR/phase.md"

    if [[ -f "$PHASE_FILE" ]]; then
        # Check if phase is not already marked as completed
        CURRENT_PHASE_STATUS=$(grep -i "| Status |" "$PHASE_FILE" | head -1)

        if [[ "$CURRENT_PHASE_STATUS" != *"completed"* && "$CURRENT_PHASE_STATUS" != *"✅"* ]]; then
            # Update phase status
            update_phase_status "$PHASE_FILE" "completed"

            # Try to update scope checklist
            update_phase_scope_checklist "$PHASE_FILE"

            # Commit phase completion
            if has_uncommitted_changes; then
                git add "$PHASE_FILE"
                git commit -m "[$PHASE_NUM-00] docs: mark phase as completed" --no-verify 2>/dev/null || true
            fi

            if ! $JSON_OUTPUT; then
                echo ""
                print_color green "🎉 Phase $PHASE_NUM marked as completed!"
            fi
        fi
    fi
fi

# Mode-specific completion actions
case "$MODE" in
    MAIN)
        # MAIN mode: Just update status, no merge needed
        if ! $JSON_OUTPUT; then
            print_color green "Task status updated to completed (MAIN mode - no merge needed)"
        fi
        ;;

    FEATURE_BRANCH)
        # FEATURE_BRANCH mode: Merge to main (unless --no-merge)
        if ! $NO_MERGE; then
            if ! $JSON_OUTPUT; then
                print_color blue "Merging to $MAIN_BRANCH..."
            fi

            # Checkout main
            git checkout "$MAIN_BRANCH" >/dev/null 2>&1

            # Merge the task branch (squash merge for cleaner history)
            if ! git merge --squash "$CURRENT_BRANCH" 2>&1; then
                output_result "error" "Merge failed. Resolve conflicts manually"
                exit 1
            fi

            # Commit the squash merge
            git commit -m "[$PHASE_NUM-$TASK_NUM] feat: $TASK_NAME" --no-verify 2>/dev/null || true

            if ! $JSON_OUTPUT; then
                print_color green "Squash merged $CURRENT_BRANCH into $MAIN_BRANCH"
            fi

            # Delete the task branch
            git branch -D "$CURRENT_BRANCH" >/dev/null 2>&1 || true

            if ! $JSON_OUTPUT; then
                print_color green "Deleted branch: $CURRENT_BRANCH"
            fi
        else
            if ! $JSON_OUTPUT; then
                print_color yellow "Skipping merge (--no-merge specified)"
                echo "To merge manually:"
                echo "  git checkout $MAIN_BRANCH && git merge --squash $CURRENT_BRANCH"
            fi
        fi
        ;;

    WORKTREE)
        # WORKTREE mode: Squash merge to main in the main repository
        if ! $NO_MERGE; then
            if ! $JSON_OUTPUT; then
                print_color blue "Squash merging to $MAIN_BRANCH (in main repo)..."
            fi

            # Get main repo path from worktree .git file
            MAIN_REPO=$(cat "$(get_repo_root)/.git" | sed 's/gitdir: //' | xargs dirname | xargs dirname)

            # In main repo: squash merge
            if ! git -C "$MAIN_REPO" merge --squash "$CURRENT_BRANCH" 2>&1; then
                output_result "error" "Merge failed in main repo. Resolve conflicts manually"
                exit 1
            fi

            git -C "$MAIN_REPO" commit -m "[$PHASE_NUM-$TASK_NUM] feat: $TASK_NAME" --no-verify 2>/dev/null || true

            if ! $JSON_OUTPUT; then
                print_color green "Squash merged $CURRENT_BRANCH into $MAIN_BRANCH (main repo)"
                echo ""
                print_color yellow "Worktree directory can be removed with: git worktree remove $(get_repo_root)"
            fi
        else
            if ! $JSON_OUTPUT; then
                print_color yellow "Skipping merge (--no-merge specified)"
            fi
        fi
        ;;
esac

# Final output
if $JSON_OUTPUT; then
    jq -n \
        --arg status "success" \
        --arg message "Task $TASK_ID completed successfully" \
        --arg task_id "$TASK_ID" \
        --arg task_name "$TASK_NAME" \
        --arg mode "$MODE" \
        --arg branch "$CURRENT_BRANCH" \
        --argjson merged "$(if $NO_MERGE || [[ "$MODE" == "MAIN" ]]; then echo false; else echo true; fi)" \
        '{status: $status, message: $message, task_id: $task_id, task_name: $task_name, mode: $mode, branch: $branch, merged: $merged}'
else
    echo ""
    print_color green "Task $TASK_ID completed successfully!"
    echo ""
    echo "Next steps:"
    echo "  - Run /task-status to see what's next"
    echo "  - Run /start-task <number> to start the next task"
fi
