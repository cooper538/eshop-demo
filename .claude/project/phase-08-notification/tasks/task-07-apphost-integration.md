# Task 7: AppHost Integration

## Metadata
| Key | Value |
|-----|-------|
| ID | task-07 |
| Status | ⚪ pending |
| Dependencies | task-05, task-06 |

## Summary
Register Notification Service in Aspire orchestration and verify end-to-end event flow.

## Scope
- [ ] Add project reference to `EShop.AppHost.csproj`
- [ ] Register `notification-service` in AppHost `Program.cs`
- [ ] Add reference to `rabbitmq` (messaging)
- [ ] Add reference to `notificationdb` (PostgreSQL database)
- [ ] Verify service starts correctly with `dotnet run --project src/AppHost`
- [ ] Verify service appears in Aspire dashboard
- [ ] Test event consumption by triggering order/stock events

## Related Specs
- → [aspire-orchestration.md](../../high-level-specs/aspire-orchestration.md) (Section: 3.2. Orchestration Code)
- → [aspire-orchestration.md](../../high-level-specs/aspire-orchestration.md) (Section: 3.3. Resource Dependencies)

---
## Notes
(Updated during implementation)
