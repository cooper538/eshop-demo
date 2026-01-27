# Task Workflow

Quick guide for working with tasks using Claude Code skills.

## Three Work Modes

```
┌─────────────────────────────────────────────────────────────────────┐
│                     THREE WORK MODES                                │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌─────────────┐  ┌──────────────────┐  ┌─────────────────────┐    │
│  │    MAIN     │  │  FEATURE_BRANCH  │  │      WORKTREE       │    │
│  │  (default)  │  │   (--branch)     │  │  (/worktree add)    │    │
│  └──────┬──────┘  └────────┬─────────┘  └──────────┬──────────┘    │
│         │                  │                       │               │
│         ▼                  ▼                       ▼               │
│   commits directly    commits on branch      commits on branch     │
│     to main           → squash merge         → squash merge        │
│                         to main                to main             │
│         │                  │                       │               │
│         ▼                  ▼                       ▼               │
│   LINEAR HISTORY     LINEAR HISTORY         LINEAR HISTORY         │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

## Mode Detection

| Check | Result |
|-------|--------|
| `.git` is a file | WORKTREE |
| branch ≠ main | FEATURE_BRANCH |
| branch = main | MAIN |

## Lifecycle Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                 MAIN MODE (default)                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  /task-status       → See what's done, what's blocking          │
│       ↓                                                         │
│  /start-task XX     → Updates status to 🔵 (stays on main)      │
│       ↓                                                         │
│  [DEVELOPMENT]      → Write code, make changes                  │
│       ↓                                                         │
│  /commit            → Commit with [XX-YY] type: format          │
│       ↓                                                         │
│  /finish-task       → Updates status to ✅                      │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│            FEATURE_BRANCH / WORKTREE MODE                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  /start-task XX --branch   OR   /worktree add task-XX           │
│       ↓                                                         │
│  [DEVELOPMENT]      → Write code, make changes                  │
│       ↓                                                         │
│  /commit            → Commit with [XX-YY] type: format          │
│       ↓                                                         │
│  /finish-task       → Squash merge to main, delete branch       │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Skills Reference

| Skill | What it does |
|-------|-------------|
| `/task-status` | Shows all tasks with status (✅🔵⚪), blocking info, progress |
| `/start-task XX` | Updates status to 🔵, stays on main (default) |
| `/start-task XX --branch` | Creates feature branch, updates status to 🔵 |
| `/finish-task` | MAIN: updates status to ✅; BRANCH: squash merge + status ✅ |
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

## Branch Naming (for feature branches/worktrees)

Format: `phase-XX/task-YY-short-description`

Examples:
- `phase-01/task-02-shared-kernel`
- `phase-02/task-03-catalog-api`

Only applies when using `--branch` or `/worktree add`.

## Status Icons

| Icon | Meaning |
|------|---------|
| ⚪ | pending - not started |
| 🔵 | in_progress - being worked on |
| ✅ | completed - done and merged |

## How to Know Your Current Phase/Task

### 1. From `/task-status` output
```
Phase 01: Foundation
━━━━━━━━━━━━━━━━━━━━━
✅ task-01 Solution Setup
🔵 task-02 Shared Kernel    ← YOU ARE HERE
⚪ task-03 Contracts
```
The `← YOU ARE HERE` marker shows your current task (from in_progress status or branch name).

### 2. From branch name (if on feature branch)
```bash
git branch --show-current
# Output: phase-01/task-02-shared-kernel
#         ↑↑       ↑↑
#         Phase    Task
```

### 3. From project structure
Tasks live in `.claude/project/phase-XX-name/tasks/`

## Parallel Work (Worktrees)

When you need to work on multiple tasks simultaneously:

```
/worktree add task-03
```

This creates a separate directory with its own branch. Use `/finish-task` to squash merge back to main.

## Tips

- Always run `/task-status` before starting to see dependencies
- Don't skip dependencies - tasks may depend on code from earlier tasks
- Use `/commit` regularly to keep progress saved
- If tests fail in `/finish-task`, fix them before retrying
