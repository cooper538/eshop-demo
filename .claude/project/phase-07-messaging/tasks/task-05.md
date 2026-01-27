# Task 05: MassTransit CorrelationId Filters

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | :white_circle: pending |
| Dependencies | - |

## Summary
Implement MassTransit publish/consume filters for automatic CorrelationId propagation via message headers.

## Scope
- [ ] Create `CorrelationIdPublishFilter<T>` that adds CorrelationId to message headers
- [ ] Create `CorrelationIdSendFilter<T>` for Send operations
- [ ] Create `CorrelationIdConsumeFilter<T>` that extracts CorrelationId and sets ambient context
- [ ] Create `MassTransitCorrelationExtensions` with `UseCorrelationIdFilters()` extension method
- [ ] Update Notification Service MassTransit config to use `UseCorrelationIdFilters()`
- [ ] Verify solution builds successfully

## Related Specs
- → [correlation-id-flow.md](../../high-level-specs/correlation-id-flow.md) (Section 6: Messaging Layer)

---
## Notes
(Updated during implementation)
