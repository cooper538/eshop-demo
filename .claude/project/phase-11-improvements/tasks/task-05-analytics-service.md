# Task 5: Analytics Service

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | ⚪ pending |
| Dependencies | - |

## Summary
Create Analytics microservice that subscribes to OrderConfirmedEvent and logs processing for observability.

## Scope
- [ ] Create Analytics service project structure (Worker service)
- [ ] Configure Aspire AppHost registration with database and messaging
- [ ] Add ResourceNames entry for analytics database
- [ ] Implement `OrderConfirmedConsumer` that logs event processing
- [ ] Add YAML configuration and settings classes
- [ ] Create EF Core DbContext with ProcessedMessages for idempotency
- [ ] Add initial migration
- [ ] Verify pub-sub: Order publishes → both Notification AND Analytics receive

## Related Specs
- → [task-05-analytics-service-spec.md](./task-05-analytics-service-spec.md) (Full implementation details)

---
## Notes
(Updated during implementation)
