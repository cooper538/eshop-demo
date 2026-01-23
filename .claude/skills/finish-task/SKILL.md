---
name: finish-task
description: Finish current task. Use when user says "dokonči task", "task hotový", "mergni do main", or runs /finish-task.
allowed-tools: Bash, Read, AskUserQuestion
---

# Finish Task

Complete the current task by running tests, updating status, and merging to main.

## Usage

```
/finish-task              # Complete current task
/finish-task --no-merge   # Complete without merge to main
/finish-task --no-test    # Complete without running tests
```

## Current State

Current branch:
!git branch --show-current

Uncommitted changes:
!git status --porcelain

## Process

### Step 1: Detect Current Task

The script reads the current branch name and extracts phase/task numbers:
- Branch must match pattern: `phase-XX/task-YY-description`
- If not on a task branch, show error

### Step 2: Check Prerequisites

Before finishing:
1. No uncommitted changes (or use `--force` to override)
2. Task file exists and is readable

### Step 3: Run Tests

Unless `--no-test` is specified:

```bash
# Unit tests (always run)
dotnet test src/EShopDemo.sln --filter "Category!=Integration"

# Integration tests (commented out, run manually when needed)
# dotnet test src/EShopDemo.sln --filter "Category=Integration"
```

If tests fail, the script stops and reports the error.

### Step 4: Check Code Formatting

```bash
dotnet csharpier . --check
```

If formatting check fails, suggest running `dotnet csharpier .` to fix.

### Step 5: Update Task Status

The script updates the task file:
- Changes `| Status | 🔵 in_progress |` to `| Status | ✅ completed |`
- Commits this change with message: `[XX-YY] docs: mark task as completed`

### Step 6: Rebase and Merge to Main

Unless `--no-merge` is specified:

```bash
# First, rebase task branch onto latest main
git rebase main

# Then switch to main and fast-forward merge
git checkout main
git merge <task-branch>
git branch -d <task-branch>
```

This keeps a linear history without merge commits. The rebase ensures task commits are on top of main.

## Arguments

- `--no-merge` - Skip merge to main (useful for review/PR workflow)
- `--no-test` - Skip running tests
- `--force` - Continue even with uncommitted changes

## Output

On success:
```
Finishing task: task-02 - Shared Kernel
Branch: phase-01/task-02-shared-kernel

Running unit tests...
Unit tests passed
Checking code formatting...
Code formatting OK
Task status updated to completed
Rebasing to main...
Rebased phase-01/task-02-shared-kernel onto main
Deleted branch: phase-01/task-02-shared-kernel

Task task-02 completed successfully!

Next steps:
  - Run /task-status to see what's next
  - Run /start-task <number> to start the next task
```

On failure:
```
Error: Unit tests failed
Fix the failing tests before completing the task.
```

## Safety Rules

1. NEVER rebase with uncommitted changes (unless --force)
2. ALWAYS run tests before marking complete (unless --no-test)
3. ALWAYS check formatting before rebase
4. NEVER delete branch on rebase failure
5. Use rebase to maintain linear history (no merge commits)

## Integration

This skill is part of the task lifecycle:
1. `/task-status` - see what's available
2. `/start-task XX` - begin working
3. `/commit` - commit changes
4. `/finish-task` - complete and merge
