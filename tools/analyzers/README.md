# Code Analyzers

Collection of analysis tools for the EShop Demo project.

## Available Analyzers

| Analyzer | Command | Description |
|----------|---------|-------------|
| Unused Packages | `./unused-packages/analyze.sh` | Detects unused NuGet package references |
| Code Quality | `./code-quality/analyze.sh` | Checks code style and Roslyn analyzer warnings |
| Security | `./security/analyze.sh` | Scans for known CVEs in dependencies |

## Usage

Run all analyzers:
```bash
./tools/analyzers/run-all.sh
```

Run specific analyzer:
```bash
./tools/analyzers/unused-packages/analyze.sh
./tools/analyzers/code-quality/analyze.sh
./tools/analyzers/security/analyze.sh
```

Or use Claude Code skill:
```bash
/analyze              # Run all
/analyze packages     # Unused packages only
/analyze quality      # Code quality only
/analyze security     # Security scan only
```

## Requirements

Analyzers require dotnet tools to be installed:
```bash
dotnet tool restore
```
