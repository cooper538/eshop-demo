# EShop Demo - Analýza Repository

Komplexní dokument popisující strukturu, účel a způsob vývoje projektu pomocí specification-driven AI development.

---

## 1. Účel Projektu

**EShop Demo** je demonstrační mikroservisní architektura postavená na .NET 10, která ukazuje produkční vzory a best practices v omezeném, ale hloubkově propracovaném rozsahu.

### Cíle

| Cíl | Popis |
|-----|-------|
| **Vzdělávací** | Demonstrace moderní .NET mikroservisní architektury |
| **Architektonický** | Ukázka gRPC komunikace, event-driven patterns, Clean Architecture |
| **Praktický** | Minimální funkční scope s maximální architektonickou hloubkou |

### Byznys Doména

Zjednodušený e-shop systém se čtyřmi službami:

```
┌─────────────────┐
│   API Gateway   │ ← Vstupní bod (YARP reverse proxy)
└────────┬────────┘
         │
    ┌────┴────┐
    ▼         ▼
┌────────┐ ┌────────┐
│Product │◄│ Order  │ ← gRPC synchronní komunikace
│Service │ │Service │
└────┬───┘ └────┬───┘
     │          │
     ▼          ▼
┌──────────────────┐
│    RabbitMQ      │ ← Asynchronní messaging
└────────┬─────────┘
         ▼
┌──────────────────┐
│  Notification    │ ← Worker service
│     Service      │
└──────────────────┘
```

---

## 2. Technologický Stack

| Kategorie | Technologie | Verze |
|-----------|-------------|-------|
| **Framework** | .NET | 10.0 |
| **Orchestrace** | .NET Aspire | 9.0+ |
| **RPC** | gRPC | 2.71+ |
| **Messaging** | MassTransit + RabbitMQ | 8.4+ |
| **Databáze** | PostgreSQL + EF Core | 16 / 10.0 |
| **API Gateway** | YARP | 2.3 |
| **Resilience** | Polly | 8.x |
| **Validace** | FluentValidation | 11.x |
| **Mediator** | MediatR | 12.x |
| **Mapování** | Riok.Mapperly | 4.1 |
| **Testování** | xUnit, MSTest, Moq, FluentAssertions | - |

---

## 3. Struktura Repository

### 3.1 Adresářová Struktura

```
eshop-demo/
├── .claude/                    # AI-driven development konfigurace
│   ├── agents/                 # Specializovaní AI agenti
│   ├── skills/                 # Vlastní příkazy (slash commands)
│   ├── scripts/                # Podpůrné skripty
│   └── project/                # Specifikace, fáze, úkoly
│       ├── high-level-specs/   # Architektonické specifikace
│       ├── phase-01-*/         # Fázové specifikace s úkoly
│       └── roadmap.md          # Přehled fází projektu
│
├── .github/workflows/          # CI/CD pipeline
├── docs/                       # Dokumentace
├── src/                        # Zdrojový kód
│   ├── AppHost/                # Aspire orchestrátor
│   ├── ServiceDefaults/        # Sdílená Aspire konfigurace
│   ├── Common/                 # Sdílené knihovny
│   │   ├── EShop.SharedKernel/ # DDD stavební bloky (zero deps)
│   │   ├── EShop.Contracts/    # Integrační eventy, DTO
│   │   ├── EShop.Grpc/         # Proto definice
│   │   ├── EShop.Common/       # Middleware, behaviors
│   │   └── EShop.ServiceClients/ # gRPC klient abstrakce
│   └── Services/               # Mikroservisy
│       ├── Products/           # Product Service (4 vrstvy)
│       ├── Order/              # Order Service (4 vrstvy)
│       ├── Notification/       # Notification Worker
│       └── Gateway/            # API Gateway
│
├── tests/                      # Testy
│   ├── EShop.ArchitectureTests/
│   └── Common.UnitTests/
│
├── tools/analyzers/            # Analytické skripty
├── Directory.Build.props       # Globální build properties
├── Directory.Packages.props    # Centralizované NuGet verze
├── EShopDemo.sln               # Solution soubor
├── CLAUDE.md                   # Pravidla pro AI vývoj
└── README.md                   # Přehled projektu
```

### 3.2 Projekty v Solution (20 projektů)

| Kategorie | Projekt | Účel |
|-----------|---------|------|
| **Orchestrace** | EShop.AppHost | Aspire orchestrátor všech služeb |
| **Infrastruktura** | EShop.ServiceDefaults | Sdílená Aspire konfigurace, OpenTelemetry |
| **Sdílené** | EShop.SharedKernel | DDD base classes (Entity, AggregateRoot, ValueObject) |
| | EShop.Contracts | Integrační eventy, DTO, service client interfaces |
| | EShop.Grpc | Proto definice pro gRPC |
| | EShop.Common | Middleware, MediatR behaviors, exceptions |
| | EShop.ServiceClients | gRPC klient implementace |
| | EShop.RoslynAnalyzers | Vlastní code analyzery |
| **Product** | Products.Domain | Agregáty, entity, domain events |
| | Products.Application | Commands, Queries, Handlers |
| | Products.Infrastructure | EF Core, migrace |
| | Products.API | REST + gRPC endpointy |
| **Order** | Order.Domain | Order agregát, entity |
| | Order.Application | Commands, Queries, Handlers |
| | Order.Infrastructure | EF Core, migrace |
| | Order.API | REST endpointy |
| **Ostatní** | Gateway.API | YARP reverse proxy |
| | EShop.NotificationService | MassTransit worker |
| **Testy** | EShop.ArchitectureTests | Architektonické testy (NetArchTest) |
| | Common.UnitTests | Unit testy sdílených knihoven |

---

## 4. Architektonické Vzory

### 4.1 Clean Architecture

Každá služba dodržuje 4-vrstvou architekturu:

```
┌─────────────────────────────────┐
│           API Layer             │ ← Controllers, gRPC services
├─────────────────────────────────┤
│       Application Layer         │ ← Commands, Queries, Handlers
├─────────────────────────────────┤
│         Domain Layer            │ ← Aggregates, Entities, Events
├─────────────────────────────────┤
│      Infrastructure Layer       │ ← EF Core, External services
└─────────────────────────────────┘

Dependency flow: API → Application → Domain ← Infrastructure
```

### 4.2 Domain-Driven Design (DDD)

| Koncept | Implementace | Příklad |
|---------|--------------|---------|
| **Aggregate Root** | `AggregateRoot` base class | `OrderEntity`, `ProductEntity` |
| **Entity** | `Entity` base class | `OrderItemEntity`, `StockEntity` |
| **Value Object** | `ValueObject` base class | `AddressVO`, `MoneyVO` |
| **Domain Event** | `DomainEventBase` record | `OrderConfirmedDomainEvent` |
| **Guard Clauses** | `Guard` static class | `Guard.Against.NullOrEmpty()` |

### 4.3 CQRS (Command Query Responsibility Segregation)

```csharp
// Commands (write operations)
public record CreateOrderCommand(OrderDto Order) : ICommand<Guid>;

// Queries (read operations)
public record GetOrderByIdQuery(Guid Id) : IQuery<OrderDto>;
```

**MediatR Pipeline Behaviors:**
- `ValidationBehavior` - FluentValidation
- `UnitOfWorkBehavior` - Domain event dispatch + SaveChanges
- `QueryTrackingBehavior` - NoTracking pro queries
- `LoggingBehavior` - Request/response logging

### 4.4 Event-Driven Architecture

**Synchronní komunikace (gRPC):**
```
Order Service → IProductServiceClient → Product Service
                     ↓
              GetProducts()
              ReserveStock()
              ReleaseStock()
```

**Asynchronní komunikace (RabbitMQ):**
```
Domain Event → Outbox Table → MassTransit → RabbitMQ → Consumer
```

**Outbox Pattern:**
- Eventy uloženy v DB společně s entity změnami
- MassTransit polluje outbox tabulku
- Garantuje at-least-once delivery

**Inbox Pattern (Idempotent Consumer):**
- Sleduje zpracované message IDs
- Prevence duplicitního zpracování

### 4.5 Resilience Patterns

| Pattern | Implementace | Účel |
|---------|--------------|------|
| **Retry** | Polly exponential backoff | Dočasné selhání |
| **Circuit Breaker** | Polly circuit breaker | Kaskádové selhání |
| **Timeout** | gRPC deadlines | Propagace timeoutu |
| **Stock TTL** | 15-min expiration job | Orphan reservations |

---

## 5. Specification-Driven AI Development

### 5.1 Filosofie

Projekt využívá **specification-driven development** řízený AI asistentem (Claude Code) s těmito principy:

| Princip | Popis |
|---------|-------|
| **Vše dokumentováno** | Žádný kód bez specifikace |
| **Žádná duplikace** | Task soubory odkazují na specs |
| **Schvalovací brány** | Žádné soubory bez explicitního souhlasu |
| **Jasné závislosti** | Topologické řazení úkolů |
| **Kontinuální validace** | Testy + formátování před merge |
| **Separace** | Agenti = strategie, Skills = exekuce |

### 5.2 AI Agenti

Tři specializovaní agenti v `.claude/agents/`:

| Agent | Účel | Kdy použít |
|-------|------|------------|
| **dotnet-assignment-planner** | Rozdělení projektu na fáze | Sprint planning, roadmaps |
| **dotnet-tech-lead** | Transformace požadavků na úkoly | User stories → tasks |
| **microservice-system-architect** | Architektonická rozhodnutí | Service boundaries, DDD |

### 5.3 Skills (Vlastní Příkazy)

11 skills v `.claude/skills/`:

| Skill | Účel |
|-------|------|
| `/task-status` | Přehled úkolů se stavy (✅🔵⚪) |
| `/start-task XX` | Zahájení práce na úkolu |
| `/commit` | Smart commit s formátem `[XX-YY] type:` |
| `/finish-task` | Dokončení úkolu (testy + merge) |
| `/sort-tasks` | Topologické řazení závislostí |
| `/worktree` | Správa worktrees pro paralelní práci |
| `/review-task` | Tech lead code review |
| `/phase-breakdown` | Rozpad fáze na úkoly |
| `/finish-phase` | Manuální dokončení fáze |
| `/analyze` | Spuštění code analyzers |
| `/create-skill` | Vytvoření nového skillu |

### 5.4 Pracovní Módy

| Mód | Detekce | Workflow |
|-----|---------|----------|
| **MAIN** | `branch = main` | Přímé commity do main |
| **FEATURE_BRANCH** | `branch ≠ main`, ne worktree | Squash merge při finish |
| **WORKTREE** | `.git` je soubor | Paralelní práce, squash merge |

### 5.5 Struktura Specifikací

```
.claude/project/
├── roadmap.md                      # 11 fází, progress tracking
├── implementation-notes.md         # Poznámky k úkolům
├── high-level-specs/               # Architektonické specifikace
│   ├── README.md                   # Přehled projektu
│   ├── shared-projects.md          # Sdílené knihovny
│   ├── product-service-interface.md
│   ├── order-service-interface.md
│   ├── grpc-communication.md
│   ├── messaging-communication.md
│   ├── correlation-id-flow.md
│   ├── aspire-orchestration.md
│   ├── configuration-management.md
│   ├── error-handling.md
│   ├── unit-testing.md
│   └── functional-testing.md
│
└── phase-XX-name/                  # Fázová složka
    ├── phase.md                    # Specifikace fáze
    └── tasks/                      # Jednotlivé úkoly
        ├── task-01-name.md
        ├── task-02-name.md
        └── ...
```

### 5.6 Formát Task Souboru

```markdown
# Task X: [Název]

## Metadata
| Key | Value |
|-----|-------|
| ID | task-XX |
| Status | ⚪ pending / 🔵 in_progress / ✅ completed |
| Dependencies | task-YY, task-ZZ |

## Summary
Jednověté shrnutí úkolu.

## Scope
- [ ] Checkbox položky (implementační kroky)

## Related Specs
- → [spec-name.md](../../high-level-specs/spec-name.md) (Section: XYZ)

## Notes
(Aktualizováno během implementace)
```

### 5.7 Commit Message Formát

```
[XX-YY] <type>: <description>

XX = číslo fáze (01, 02, ...)
YY = číslo úkolu (01, 02, ...)
type = feat | fix | docs | meta
description = imperativ, max 50 znaků
```

**Příklady:**
- `[01-02] feat: implement Product entity`
- `[03-05] fix: resolve stock reservation race condition`
- `[00-00] meta: update CI pipeline` (pro non-task změny)

---

## 6. Fáze Projektu

Projekt je rozdělen do 11 fází:

| Fáze | Název | Status | Úkoly |
|------|-------|--------|-------|
| 01 | Foundation | ✅ Completed | 6 |
| 02 | Aspire | ✅ Completed | 4 |
| 03 | Product Core | ✅ Completed | 8 |
| 04 | Product Internal | 🔵 In Progress | 8 |
| 05 | Order Core | ⚪ Pending | - |
| 06 | Order Integration | ⚪ Pending | - |
| 07 | Messaging | ⚪ Pending | - |
| 08 | Notification | ⚪ Pending | - |
| 09 | Gateway | ⚪ Pending | - |
| 10 | Testing | ⚪ Pending | - |
| 11 | Improvements | ⚪ Pending | - |

**Aktuální progress:** 27% (3/11 fází dokončeno)

---

## 7. Vývojový Workflow

### 7.1 Typický Pracovní Cyklus

```
1. /task-status              → Zobrazení dostupných úkolů
2. /start-task XX            → Zahájení úkolu (validace závislostí)
   [--branch]                → Volitelně na feature branch
3. [VÝVOJ]                   → Psaní kódu podle specs
4. /commit                   → Smart commit s auto-formátem
5. /review-task              → Volitelná tech lead review
6. /finish-task              → Dokončení (testy + formátování)
   [--no-test]               → Přeskočení testů (opatrně)
```

### 7.2 Bezpečnostní Brány

| Akce | Vyžaduje souhlas |
|------|------------------|
| Vytvoření souborů | ✅ Ano |
| Commit změn | ✅ Ano (přes `/commit`) |
| Dokončení úkolu | ✅ Ano |
| Destruktivní git operace | ✅ Ano |
| Force push | ⛔ Zakázáno na main |

### 7.3 Research Workflow

Při zahájení úkolu (`/start-task XX`):

1. **Analýza codebase** - Glob/Grep pro podobné patterns
2. **Web research** - Oficiální dokumentace, best practices
3. **Čtení specs** - Všechny "Related Specs" z task souboru
4. **Návrh** - Prezentace implementačního přístupu
5. **Schválení** - Čekání na potvrzení uživatelem

---

## 8. CI/CD Pipeline

### 8.1 GitHub Actions Workflow

```yaml
# .github/workflows/ci.yml

Jobs:
1. Build
   - dotnet restore
   - dotnet build (Release)
   - dotnet test (Architecture tests)

2. Code Quality
   - CSharpier check
   - dotnet format check

3. Security
   - Vulnerable packages check
   - Deprecated packages warning
   - Unused packages detection
```

### 8.2 Pre-push Hook

```bash
# .githooks/pre-push
dotnet format --verify-no-changes
dotnet csharpier check .
```

---

## 9. Architektonická Rozhodnutí

| Rozhodnutí | Důvod | Trade-off |
|------------|-------|-----------|
| **Bez Repository Pattern** | Přímý DbContext, plný EF power | Těsnější vazba na EF |
| **SharedKernel bez závislostí** | Čisté DDD, žádné external deps | Omezeno na core koncepty |
| **Synchronní stock rezervace** | Okamžitá UX, snazší debugging | Service coupling |
| **Fat events** (include data) | Nezávislost Notification service | Občas stale data |
| **gRPC pro interní API** | HTTP/2, strong typing, efektivní | Nevhodné pro external clients |
| **YAML konfigurace** | Human-readable, komentáře | Více souborů |

---

## 10. Spuštění Projektu

### Lokální Vývoj (Aspire)

```bash
# Spuštění všech služeb
dotnet run --project src/AppHost

# Otevře se Aspire Dashboard s:
# - Všemi mikroservisy
# - PostgreSQL + pgAdmin
# - RabbitMQ + Management UI
# - Traces, Logs, Metrics
```

### Jednotlivé Příkazy

| Akce | Příkaz |
|------|--------|
| Build | `dotnet build EShopDemo.sln` |
| Test | `dotnet test EShopDemo.sln` |
| Format | `dotnet csharpier format .` |
| Format check | `dotnet csharpier check .` |
| Analyzery | `./tools/analyzers/run-all.sh` |

---

## 11. Závěr

**EShop Demo** představuje moderní přístup k vývoji mikroservisních aplikací s těmito klíčovými charakteristikami:

1. **Specification-Driven Development** - Vše začíná specifikací
2. **AI-Assisted Workflow** - Claude Code jako vývojový partner
3. **Production-Grade Patterns** - DDD, CQRS, Event-Driven, Clean Architecture
4. **Automatizovaná kvalita** - Testy, formátování, code review
5. **Transparentní progress** - Fáze, úkoly, stavy, závislosti

Projekt slouží jako referenční implementace i učební materiál pro moderní .NET vývoj.

---

*Vygenerováno: 2026-01-30*
*Verze: 1.0*
