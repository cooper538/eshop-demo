# CRITICAL RULES (Never Break)

- **NEVER** commit without explicit user approval (handled by `/commit`)
- **ALWAYS** stop and ask before completing a task
- **ALWAYS** use `/commit` skill for commits (ensures proper format)
- When on feature branch or worktree, use `/finish-task` for squash merge

# General

- codebase language is english only
- **Target framework**: .NET 10 (net10.0) - use this version everywhere, do not downgrade
- **NuGet packages**: Central Package Management via `Directory.Packages.props` - add versions there, not in .csproj
- **Confirmation required**: Before making significant deviations from specifications (different .NET version, different library, different design pattern, etc.), always ask user for confirmation
- **Implementation style**: Act as a senior developer - prefer pragmatic solutions with clean code following KISS, SOLID principles, proper use of inheritance, composition, and design patterns where appropriate. If something seems off or unclear, ask before implementing.

## Claude Code Configuration
All customizations in `.claude/` directory:
- `agents/` - Custom agents
- `skills/` - Custom slash commands
- `project/` - Tasks, notes, specs

## Task Workflow

### Three Work Modes

| Mode | When to use | How to start |
|------|-------------|--------------|
| **MAIN** (default) | Normal work, direct commits to main | `/start-task XX` |
| **FEATURE_BRANCH** | Isolated changes, code review needed | `/start-task XX --branch` |
| **WORKTREE** | Parallel work on multiple tasks | `/worktree add task-XX` |

### Detection Logic
- **WORKTREE**: `.git` is a file (not directory)
- **FEATURE_BRANCH**: current branch ≠ main, not worktree
- **MAIN**: current branch = main

### Branch Naming (for feature branches/worktrees)

**Format**: `phase-XX/task-YY-short-description`
- `XX` = phase number (01, 02, ...)
- `YY` = task number (01, 02, ...)
- `short-description` = max 4 words, kebab-case

### Workflow

**MAIN mode (default):**
1. `/task-status` - check what's available
2. `/start-task XX` - updates status to 🔵 (stays on main)
3. Development + `/commit` - commits directly to main
4. `/finish-task` - updates status to ✅

**FEATURE_BRANCH / WORKTREE mode:**
1. `/start-task XX --branch` or `/worktree add task-XX`
2. Development + `/commit` - commits on feature branch
3. `/finish-task` - squash merge to main, delete branch

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
| `/finish-task` | Complete task (tests, merge, auto-detect phase completion) |
| `/finish-phase XX` | Manually complete a phase |
| `/task-status` | Task status overview |
| `/commit` | Smart commit with `[XX-YY] type:` format |
| `/worktree` | Manage worktrees for parallel work |
| `/sort-tasks` | Topological task sorting |
| `/phase-breakdown` | Break down phase into tasks |
| `/analyze` | Run code analyzers (packages, quality, security) |
| `/e2e-test` | Run E2E test scenarios against running services |
| `/review-task` | Tech lead review of task implementation |
| `/create-skill` | Create a new Claude Code skill |

### When to Use Feature Branch or Worktree

**Use `--branch` when:**
- Experimental changes you might want to discard
- Need code review before merging
- Larger isolated changes

**Use `/worktree add` when:**
- Working on multiple tasks simultaneously
- Don't want to stash changes when switching
- Long-running feature across multiple sessions

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
│   ├── EShop.Common.Api/
│   ├── EShop.Common.Application/
│   ├── EShop.Common.Infrastructure/
│   ├── EShop.RoslynAnalyzers/
│   └── EShop.ServiceClients/
├── Services/
│   ├── Analytics/
│   ├── DatabaseMigration/
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
| List dependencies | `./tools/list-dependencies.sh` |

## Aspire
| Action | Command |
|--------|---------|
| Run all services | `dotnet run --project src/AppHost` |
| Dashboard | Opens automatically at https://localhost:port |

## Distributed Tracing

All services use CorrelationId for distributed tracing across HTTP, gRPC, and messaging.

| Action | Command |
|--------|---------|
| Trace request | `./tools/e2e-test/trace-correlation.sh <correlation-id>` |
| Trace all logs | `./tools/e2e-test/trace-correlation.sh <correlation-id> --all-logs` |
| JSON output | `./tools/e2e-test/trace-correlation.sh <correlation-id> --json` |

**Log format (Serilog):** `[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}`

## Code Standards
- See `docs/code-guidelines.md`
- Use CSharpier for formatting (120 char width, 4 spaces)
- All `.md` files in the repository must be in English only

## Comments

**Remove:** section comments in Program.cs, XML docs on internal classes, comments describing WHAT

**Keep:**
- XML docs on controllers (generates OpenAPI)
- `// EF Core constructor` - explains private constructor
- Business rules explaining WHY
- One-liner for patterns: `// Inbox Pattern - ensures exactly-once processing`
- Critical ordering: `// UnitOfWork MUST be LAST`
- TODO comments

