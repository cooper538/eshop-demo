# Task 04: Service Configuration Updates

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | ✅ completed |
| Dependencies | task-01, task-02, task-03 |

## Summary
Update all services to use environment-aware configuration that switches between local (Aspire) and Azure (Key Vault/PostgreSQL SSL) based on `IsProduction()`.

## Scope
- [x] Update `Product.API/Program.cs` with environment-aware configuration
- [x] Update `Order.API/Program.cs` with environment-aware configuration
- [x] Update `Notification/Program.cs` with environment-aware configuration
- [x] Update `Gateway.API/Program.cs` with environment-aware configuration
- [x] Add `*.settings.Production.yaml` to each service with Azure-specific configuration

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 11. Aspire Integration)
- -> [aspire-orchestration.md](../high-level-specs/aspire-orchestration.md)

---
## Notes
- Messaging (RabbitMQ) requires no changes - same config works in both environments
- Environment detection uses built-in `IsProduction()` - no custom code needed
- Config files: `*.settings.Production.yaml`
