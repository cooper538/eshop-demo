# Versioning Guidelines

## Commit Message Format

```
[phase-XX/task-YY] <type>: <description>
```

### Types
- `feat` - new feature
- `fix` - bug fix
- `docs` - documentation

### Examples
```
[phase-01/task-02] feat: implement Product entity
[phase-01/task-02] fix: correct stock validation
[phase-02/task-01] feat: add Order aggregate root
```

---

## Trunk-Based Development

- Primary development on `main` branch
- Short-lived feature branches (1-2 days max)
- Frequent integration to main
- No long-running branches

---

## Rebase Strategy

- Always rebase before merging to main
- Squash related commits into logical units
- Maintain linear history
- Never rebase shared/pushed commits

```bash
# Update feature branch before merge
git fetch origin
git rebase origin/main

# Interactive rebase to squash
git rebase -i origin/main
```
