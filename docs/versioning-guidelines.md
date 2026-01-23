# Versioning Guidelines

## Commit Message Format

```
[XX-YY] <type>: <description>
```

Where:
- `XX` = phase number (01, 02, ...)
- `YY` = task number (01, 02, ...)

### Types
- `feat` - new feature
- `fix` - bug fix
- `docs` - documentation

### Examples
```
[01-02] feat: implement Product entity
[01-02] fix: correct stock validation
[02-01] feat: add Order aggregate root
[01-05] docs: add implementation notes
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
