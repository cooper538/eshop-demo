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

**Task lifecycle with skills:**

1. **Overview**: `/task-status` - what's done, what's blocking
2. **Start**: `/start-task XX` - validates deps, creates branch, updates status
3. **Development**: coding, changes
4. **Commit**: `/commit` - formatted commit with `[XX-YY] type:` prefix
5. **Completion**: `/finish-task` - tests, merge, cleanup

**Alternative for parallel work:**
- `/worktree add task-XX` - new worktree for another task

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

