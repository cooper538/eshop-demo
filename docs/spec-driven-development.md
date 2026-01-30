# Specification-Driven AI Development

This document explains the development methodology used to build this project.

## Contents

1. [What is Spec-Driven AI Development?](#what-is-spec-driven-ai-development)
2. [How It Works](#how-it-works)
3. [Specification Structure](#specification-structure)
4. [Development Workflow](#development-workflow)
5. [Key Principles](#key-principles)
6. [Benefits](#benefits)

---

## What is Spec-Driven AI Development?

A development methodology where **AI acts as a senior developer** guided by structured
specifications, with humans providing oversight and approval at key checkpoints.

```
Traditional Development:
    Human writes code → Human reviews → Merge

Spec-Driven AI Development:
    Human writes spec → AI implements → Human reviews → Merge
```

The key insight: **specifications are the new source code**. Instead of writing
implementation details, developers write high-level requirements that AI translates
into working code.

---

## How It Works

### The Collaboration Model

```
┌─────────────────────────────────────────────────────────────────┐
│                        HUMAN ROLE                                │
│  • Define specifications and requirements                        │
│  • Review AI-generated code                                      │
│  • Approve commits and task completion                          │
│  • Make architectural decisions                                  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                         AI ROLE                                  │
│  • Read and understand specifications                            │
│  • Implement code following patterns                             │
│  • Maintain consistency with existing codebase                   │
│  • Ask clarifying questions when needed                          │
└─────────────────────────────────────────────────────────────────┘
```

### Phase → Task → Implementation

Work is organized hierarchically:

```
Phase (e.g., "Product Service Core")
    │
    ├── Task 01: Create domain entities
    ├── Task 02: Implement commands
    ├── Task 03: Add queries
    ├── Task 04: Configure database
    └── Task 05: Add API endpoints
```

Each task has:
- Clear acceptance criteria
- Dependencies on other tasks
- Estimated complexity
- Status tracking (⬜ → 🔵 → ✅)

---

## Specification Structure

```
specification/
├── roadmap.md                    # Project overview, phase list
├── phase-01-foundation/
│   ├── phase.md                  # Phase goals, success criteria
│   └── tasks/
│       ├── task-01.md            # Individual task specification
│       ├── task-02.md
│       └── ...
├── phase-02-product-core/
│   ├── phase.md
│   └── tasks/...
└── high-level-specs/
    ├── architecture.md           # Architectural decisions
    ├── communication.md          # Service communication patterns
    └── ...
```

### Task Specification Format

```markdown
# Task XX: Task Title

## Status: ⬜ Pending | 🔵 In Progress | ✅ Completed

## Goal
What this task achieves.

## Dependencies
- Task XX-YY (must be completed first)

## Requirements
1. Specific requirement one
2. Specific requirement two

## Acceptance Criteria
- [ ] Criterion one
- [ ] Criterion two

## Implementation Notes
Technical guidance for the AI.
```

---

## Development Workflow

### Daily Workflow

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│ /task-status│ ──► │ /start-task │ ──► │  Implement  │
└─────────────┘     └─────────────┘     └──────┬──────┘
                                               │
                                               ▼
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│ Next task?  │ ◄── │/finish-task │ ◄── │  /commit    │
└─────────────┘     └─────────────┘     └─────────────┘
```

### Commands

| Command | Purpose |
|---------|---------|
| `/task-status` | See available tasks, current progress |
| `/start-task XX` | Begin working on task, mark as 🔵 |
| `/commit` | Create commit with proper format `[XX-YY] type: message` |
| `/finish-task` | Complete task, run tests, mark as ✅ |

### Checkpoint Gates

The workflow enforces human approval at key points:

1. **Task Start** - Human selects which task to work on
2. **Commit** - Human reviews changes before committing
3. **Task Completion** - Human verifies acceptance criteria met
4. **Phase Completion** - Human confirms phase goals achieved

---

## Key Principles

### 1. Specs Before Code

Never implement without a specification. If requirements are unclear:
- Ask clarifying questions
- Update the spec first
- Then implement

### 2. AI Proposes, Human Approves

AI doesn't make autonomous decisions. At each checkpoint:
- AI presents its work
- Human reviews and approves (or requests changes)
- Only then does work proceed

### 3. Full Traceability

Every line of code traces back to:
- A task specification
- A commit with task reference `[XX-YY]`
- A phase goal

```
Spec (task-03.md) → Commit [02-03] → Code (OrderEntity.cs)
```

### 4. Consistent Patterns

AI follows established patterns from:
- Existing codebase (reads before writing)
- Code guidelines (`docs/code-guidelines.md`)
- High-level specs (`specification/high-level-specs/`)

### 5. Incremental Progress

Work in small, verifiable increments:
- One task at a time
- One commit per logical change
- Continuous verification

---

## Benefits

### For Code Quality

- **Consistent architecture** - AI follows patterns religiously
- **Complete implementations** - Specs ensure nothing is forgotten
- **Clean code** - AI optimizes for readability

### For Documentation

- **Specs ARE documentation** - Requirements live with code
- **Decision records** - Trade-offs documented in specs
- **Change history** - Tasks link commits to features

### For Velocity

- **Parallel work** - Multiple tasks can be specified ahead
- **Reduced context switching** - AI maintains project context
- **Faster implementation** - Specs eliminate ambiguity

### For Learning

- **Reproducible methodology** - Others can follow the same approach
- **Clear examples** - This project demonstrates the workflow
- **Transferable patterns** - Specs can be adapted for other projects

---

## Example: Adding a Feature

**Step 1: Write the specification**

```markdown
# Task 05: Add Order Cancellation

## Goal
Allow customers to cancel pending orders.

## Requirements
1. Only pending orders can be cancelled
2. Cancelled orders release reserved stock
3. Customer receives cancellation email

## Acceptance Criteria
- [ ] CancelOrderCommand implemented
- [ ] Stock released via gRPC call
- [ ] OrderCancelledEvent published
- [ ] Integration test passes
```

**Step 2: AI implements**

AI reads the spec, examines existing patterns, and implements:
- Command and handler
- Domain logic in OrderEntity
- gRPC call to Product service
- Event publishing

**Step 3: Human reviews**

Human checks:
- Code follows patterns
- All acceptance criteria met
- No security issues

**Step 4: Commit and complete**

```bash
/commit    # [03-05] feat: add order cancellation
/finish-task
```

---

## Conclusion

Spec-Driven AI Development shifts the developer's focus from *writing code* to
*defining requirements*. The AI handles implementation details while humans
maintain control over architecture, quality, and direction.

This project serves as a reference implementation of this methodology.
