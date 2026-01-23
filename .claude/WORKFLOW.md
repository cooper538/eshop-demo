# Task Workflow

Quick guide for working with tasks using Claude Code skills.

## Lifecycle Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                      TASK LIFECYCLE                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  /task-status       → See what's done, what's blocking          │
│       ↓                                                         │
│  /start-task XX     → Start task (validates deps, creates       │
│       │               branch, updates status to 🔵)             │
│       ↓                                                         │
│  [DEVELOPMENT]      → Write code, make changes                  │
│       ↓                                                         │
│  /commit            → Commit with [XX-YY] prefix                │
│       ↓                                                         │
│  /finish-task       → Run tests, merge to main,                 │
│                       update status to ✅                       │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Skills Reference

| Skill | What it does |
|-------|-------------|
| `/task-status` | Shows all tasks with status (✅🔵⚪), blocking info, progress |
| `/start-task XX` | Validates dependencies, creates branch, starts task |
| `/finish-task` | Runs tests, checks formatting, merges, marks complete |
| `/commit` | Smart commit with [XX-YY] type: description format |
| `/sort-tasks` | Shows topological order and entry points |
| `/worktree add task-XX` | Creates parallel worktree for another task |

## Quick Start

### 1. Check what's available
```
/task-status
```

### 2. Start working
```
/start-task 01
```

### 3. Make your changes
Write code, create files, do the work.

### 4. Commit progress
```
/commit
```

### 5. Finish when done
```
/finish-task
```

## Branch Naming

Format: `phase-XX/task-YY-short-description`

Examples:
- `phase-01/task-02-shared-kernel`
- `phase-02/task-03-catalog-api`

## Status Icons

| Icon | Meaning |
|------|---------|
| ⚪ | pending - not started |
| 🔵 | in_progress - being worked on |
| ✅ | completed - done and merged |

## How to Know Your Current Phase/Task

### 1. From branch name
```bash
git branch --show-current
# Output: phase-01/task-02-shared-kernel
#         ↑↑       ↑↑
#         Phase    Task
```

### 2. From `/task-status` output
```
Phase 01: Foundation
━━━━━━━━━━━━━━━━━━━━━
✅ task-01 Solution Setup
🔵 task-02 Shared Kernel    ← YOU ARE HERE
⚪ task-03 Contracts
```
The `← YOU ARE HERE` marker shows your current task based on branch.

### 3. From project structure
Tasks live in `.claude/project/phase-XX-name/tasks/`

## Parallel Work

When you need to work on multiple tasks simultaneously:

```
/worktree add task-03
```

This creates a separate directory with its own branch, so you can switch between tasks without stashing.

## Tips

- Always run `/task-status` before starting to see dependencies
- Don't skip dependencies - tasks may depend on code from earlier tasks
- Use `/commit` regularly to keep progress saved
- If tests fail in `/finish-task`, fix them before retrying
