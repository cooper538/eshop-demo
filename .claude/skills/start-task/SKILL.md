---
name: start-task
description: Start work on a task. Use when user says "začni práci na task-XX", "chci pracovat na task-XX", "start task XX", or runs /start-task.
allowed-tools: Bash, Read, AskUserQuestion
---

# Start Task

Validate dependencies, create feature branch, and update task status to start working on a task.

## Usage

```
/start-task 02                              # Task 02 in current phase
/start-task task-02                         # Explicit task-02
/start-task phase-01/task-02-shared-kernel  # Full branch name
```

## Current State

Current branch:
!git branch --show-current

Uncommitted changes:
!git status --porcelain

## Process

### Step 1: Parse Task Identifier

The script accepts multiple formats:
- `02` or `2` - just task number (uses current or default phase)
- `task-02` - explicit task ID
- `phase-01/task-02-description` - full branch name

### Step 2: Validate Dependencies

Before starting, the script checks:
1. Task file exists in `.claude/project/phase-XX-*/tasks/`
2. Task is not already in_progress or completed
3. All dependencies are completed (status: ✅)

If blocked, show which tasks need to be completed first.

### Step 3: Create/Switch Branch

```bash
.claude/scripts/start-task.sh <task-identifier>
```

The script will:
- Generate branch name from task file: `phase-XX/task-YY-description`
- Create new branch from main if doesn't exist
- Switch to existing branch if it already exists

### Step 4: Update Task Status

The script automatically updates the task file:
- Changes `| Status | ⚪ pending |` to `| Status | 🔵 in_progress |`

### Step 5: Show Task Scope

After starting, display the task's scope section for context.

## Arguments

- `$ARGUMENTS` - Task identifier in any supported format

## Output

On success:
```
Started task task-02: Shared Kernel
Branch: phase-01/task-02-shared-kernel
Task file: .claude/project/phase-01-foundation/tasks/task-02-shared-kernel.md

Task scope:
- [ ] Implement Entity base class
- [ ] Implement ValueObject base class
...
```

On error (blocked):
```
Error: Task task-03 is blocked by: task-02
Complete the blocking tasks first or use /task-status to see the full picture.
```

## Safety Rules

1. NEVER start a task with incomplete dependencies
2. ALWAYS show uncommitted changes warning if present
3. NEVER force-checkout if there are uncommitted changes
4. ALWAYS preserve existing branch if it exists

## Integration

After starting a task:
- Use `/commit` to commit changes with proper [XX-YY] prefix
- Use `/finish-task` when done to complete and merge
- Use `/task-status` to check progress
