# Task 04: Key Vault Module

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | pending |
| Dependencies | task-03 |

## Summary
Create Bicep module for Azure Key Vault with connection string secrets for PostgreSQL and RabbitMQ.

## Scope
- [ ] Create `key-vault.bicep` module with Standard SKU
- [ ] Configure RBAC authorization mode (not access policies)
- [ ] Store PostgreSQL connection strings for all databases
- [ ] Store RabbitMQ connection string
- [ ] Grant Key Vault Secrets User role to managed identity
- [ ] Enable soft delete with 7-day retention
- [ ] Disable purge protection for demo cleanup capability

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 5.2 Key Vault)

---
## Notes
- Secret names use `--` separator (e.g., `ConnectionStrings--productdb`)
- RBAC authorization preferred over access policies
