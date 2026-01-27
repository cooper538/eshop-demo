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

## Work Modes

### MAIN (Default)
- Commits go directly to `main` branch
- Simplest workflow for most tasks
- Linear history maintained naturally

### Feature Branch (Optional)
- Use `/start-task XX --branch` to create
- All commits stay on feature branch
- Squash merge to main when done via `/finish-task`

### Worktree (Parallel Work)
- Use `/worktree add task-XX` to create
- Separate directory with its own branch
- Squash merge to main when done via `/finish-task`

---

## Squash Merge Strategy

When using feature branches or worktrees:
- All branch commits are combined into one commit on main
- Maintains clean, linear history
- Each task = one commit on main

```bash
# Squash merge (done automatically by /finish-task)
git checkout main
git merge --squash <feature-branch>
git commit -m "[XX-YY] feat: <summary of all changes>"
git branch -d <feature-branch>
```
