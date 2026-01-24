# CRITICAL RULES (Never Break)

- **NEVER** run `git commit` directly - ALWAYS use `/commit` skill
- **NEVER** run `git merge` or `git rebase` directly - ALWAYS use `/finish-task` skill
- **NEVER** commit without explicit user approval (handled by `/commit`)
- **ALWAYS** stop and ask before completing a task

# General

- codebase language is english only
- **Confirmation required**: Before making significant deviations from specifications (different .NET version, different library, different design pattern, etc.), always ask user for confirmation
- **Implementation style**: Act as a senior developer - prefer pragmatic solutions with clean code following KISS, SOLID principles, proper use of inheritance, composition, and design patterns where appropriate. If something seems off or unclear, ask before implementing.

## Claude Code Configuration
All customizations in `.claude/` directory:
- `agents/` - Custom agents
- `skills/` - Custom slash commands
- `project/` - Tasks, notes, specs

## Task Workflow

### Branch Naming Convention
Work on a task MUST happen in a feature branch with the following convention:

**Format**: `phase-XX/task-YY-short-description`
- `XX` = phase number (01, 02, ...)
- `YY` = task number (01, 02, ...)
- `short-description` = max 4 words, kebab-case

**Examples**:
- `phase-01/task-02-shared-kernel`
- `phase-01/task-03-contracts-events`
- `phase-02/task-01-aspire-setup`

### Workflow

**Task lifecycle (MANDATORY skills):**

1. `/task-status` - check what's available
2. `/start-task XX` - creates branch, updates status
3. Development - write code, make changes
4. **`/commit`** - REQUIRED for all commits (asks for approval)
5. **`/finish-task`** - REQUIRED to complete (runs tests, rebase, merge)

**FORBIDDEN**: Direct `git commit`, `git merge`, `git rebase` commands

**Alternative for parallel work:**
- `/worktree add task-XX` - new worktree for another task

### Checkpoints (Stop and Confirm)

Before these actions, ALWAYS ask user for confirmation:
- Committing changes (handled by `/commit` skill)
- Completing a task (handled by `/finish-task` skill)
- When user says "pokračuj" / "continue" on multiple tasks

### Batch Work Rule

When user says "pokračuj" or "continue":
- Complete **ONE task at a time**
- After each task, show summary and ask: "Continue to next task?"
- NEVER auto-chain multiple tasks without confirmation

### Available Skills

| Skill | Purpose |
|-------|---------|
| `/start-task XX` | Start working on a task |
| `/finish-task` | Complete task (tests, merge) |
| `/task-status` | Task status overview |
| `/commit` | Smart commit with `[XX-YY] type:` format |
| `/worktree` | Manage worktrees for parallel work |
| `/sort-tasks` | Topological task sorting |
| `/phase-breakdown` | Break down phase into tasks |
| `/analyze` | Run code analyzers (packages, quality, security) |

### When to Use Worktree
Use `/worktree add` when:
1. **Parallel work** - working on multiple tasks simultaneously in different sessions
2. **Potential conflict** - another session is working on a task that may modify similar files
3. **Quick fix** - need to quickly fix something on main while working on a feature

### Implementation Notes

File `.claude/project/implementation-notes.md` is used for brief task notes.

**Format**: `[XX-YY] task name - note`
- `XX` = phase number
- `YY` = task number
- task name = short task identifier
- note = max 1 sentence

**When to use**:
- TODO for future (missing tests, refactor later)
- Important decision (why X instead of Y)
- Known issues (workaround, temporary solution)

**Examples**:
```
[01-05] EShop.Common - Missing tests, add later
[02-03] Order Service - Used Polly instead of custom retry logic
[03-01] Product API - Temporary workaround for EF bug #1234
```

When user says "add note to task" or "write note", add a line to this file.

# Architecture

## Patterns
- **DDD** (Domain-Driven Design) - bounded contexts, aggregates, domain events
- **Clean Architecture** - layers: Domain → Application → Infrastructure → API
- **CQRS** - separate read/write models where beneficial
- **Microservices** - independent deployable services

## Project Structure
```
src/
├── AppHost/              # .NET Aspire orchestration
├── ServiceDefaults/      # Shared Aspire configuration
├── Common/               # Shared libraries
│   ├── EShop.SharedKernel/
│   ├── EShop.Contracts/
│   ├── EShop.Grpc/
│   ├── EShop.Common/
│   └── EShop.ServiceClients/
├── Services/
│   ├── Gateway/
│   ├── Product/
│   ├── Order/
│   └── Notification/
EShopDemo.sln
```

# Development

## Solution
- Path: `EShopDemo.sln`

## Commands
| Action | Command |
|--------|---------|
| Build | `dotnet build EShopDemo.sln` |
| Test | `dotnet test EShopDemo.sln` |
| Format | `dotnet csharpier format .` |
| Format check | `dotnet csharpier check .` |
| Analyze code | `./tools/analyzers/run-all.sh` or `/analyze` |

## Aspire
| Action | Command |
|--------|---------|
| Run all services | `dotnet run --project src/AppHost` |
| Dashboard | Opens automatically at https://localhost:port |

## Code Standards
- See `docs/code-guidelines.md`
- Use CSharpier for formatting (120 char width, 4 spaces)
- All `.md` files in the repository must be in English only

