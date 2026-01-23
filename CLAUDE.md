# General
- codebase language is english only
- **Confirmation required**: Before making significant deviations from specifications (different .NET version, different library, different design pattern, etc.), always ask user for confirmation

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

1. **Přehled**: `/task-status` - co je hotové, co blokuje
2. **Start**: `/start-task XX` - validuje deps, vytvoří branch, updatne status
3. **Vývoj**: kódování, změny
4. **Commit**: `/commit` - formátovaný commit s [XX-YY] prefixem
5. **Dokončení**: `/finish-task` - testy, merge, cleanup

**Alternativa pro paralelní práci:**
- `/worktree add task-XX` - nový worktree pro jiný task

### Available Skills

| Skill | Purpose |
|-------|---------|
| `/start-task XX` | Start working on a task |
| `/finish-task` | Complete task (tests, merge) |
| `/task-status` | Task status overview |
| `/commit` | Smart commit with [XX-YY] prefix |
| `/worktree` | Manage worktrees for parallel work |
| `/sort-tasks` | Topological task sorting |
| `/analyze` | Run code analyzers (packages, quality, security) |

### When to Use Worktree
Use `/worktree add` when:
1. **Parallel work** - working on multiple tasks simultaneously in different sessions
2. **Potential conflict** - another session is working on a task that may modify similar files
3. **Quick fix** - need to quickly fix something on main while working on a feature

### Implementation Notes

Soubor `.claude/project/implementation-notes.md` slouží pro stručné poznámky k taskům.

**Formát**: `[XX-YY] název tasku - poznámka`
- `XX` = číslo fáze
- `YY` = číslo tasku
- název tasku = krátký identifikátor tasku
- poznámka = max 1 věta, česky nebo anglicky

**Kdy použít**:
- TODO pro budoucnost (chybí testy, refactor později)
- Důležité rozhodnutí (proč X místo Y)
- Known issues (workaround, dočasné řešení)

**Příklady**:
```
[01-05] EShop.Common - Chybí testy, dodělat později
[02-03] Order Service - Použit Polly místo vlastní retry logic
[03-01] Product API - Temporary workaround pro EF bug #1234
```

Když uživatel řekne "přidej poznámku k tasku" nebo "zapiš note", přidej řádku do tohoto souboru.

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
| Format | `dotnet csharpier .` |
| Format check | `dotnet csharpier . --check` |
| Analyze code | `./tools/analyzers/run-all.sh` or `/analyze` |

## Aspire
| Action | Command |
|--------|---------|
| Run all services | `dotnet run --project src/AppHost` |
| Dashboard | Opens automatically at https://localhost:port |

## Code Standards
- See `docs/code-guidelines.md`
- Use CSharpier for formatting (120 char width, 4 spaces)

