# Task 1: Project Setup

## Metadata
| Key | Value |
|-----|-------|
| ID | task-01 |
| Status | 🔵 in_progress |
| Dependencies | - |

## Summary
Create Worker Service project with Aspire integration for processing notification events.

## Scope
- [ ] Create `EShop.NotificationService` Worker Service project in `src/Services/Notification/`
- [ ] Add project reference to solution (`EShopDemo.sln`)
- [ ] Reference `EShop.ServiceDefaults` for Aspire integration
- [ ] Reference `EShop.Contracts` for integration events
- [ ] Add MassTransit packages for RabbitMQ consumer
- [ ] Implement basic `Program.cs` with `AddServiceDefaults()` and MassTransit configuration
- [ ] Configure connection to RabbitMQ via Aspire injection

## Related Specs
- → [aspire-orchestration.md](../../high-level-specs/aspire-orchestration.md) (Section: 5. Service Integration)
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section: 8.2. Consumer Configuration)

---
## Notes
(Updated during implementation)
