# Task 01: PostgreSQL SSL Configuration

## Metadata
| Key | Value |
|-----|-------|
| ID | task-01 |
| Status | ✅ completed |
| Dependencies | - |

## Summary
Add SSL mode handling for Azure PostgreSQL Flexible Server connections with automatic configuration based on environment.

## Scope
- [x] Create `AddNpgsqlDbContextAzure<T>()` extension for Azure PostgreSQL with SSL
- [x] Add `AddNpgsqlDbContextPoolAzure<T>()` for high-throughput scenarios
- [x] Configure retry policy (3 retries, max 10s delay)
- [x] Handle graceful skip when connection string is missing (design-time builds)

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 3. Data Resources)

---
## Notes
- TrustServerCertificate is obsolete in Npgsql 9.x - not used
- Extensions in `EShop.Common.Infrastructure/Data/`
