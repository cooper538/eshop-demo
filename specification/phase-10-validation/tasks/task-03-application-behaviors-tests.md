# Task 03: Application Behaviors Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Summary
Unit tests for EShop.Common.Application behaviors - the core pipeline infrastructure (UnitOfWork, domain events, correlation).

## Scope

### UnitOfWorkExecutor
- [ ] Test domain event dispatching
- [ ] Test cascading events handling
- [ ] Test max iteration protection (infinite loop)
- [ ] Test concurrency exception mapping

### DomainEventDispatchHelper
- [ ] Test event collection from aggregates
- [ ] Test event clearing before dispatch
- [ ] Test multiple aggregates handling

### MediatRDomainEventDispatcher
- [ ] Test event wrapping in DomainEventNotification
- [ ] Test multiple events publishing

### CorrelationContext
- [ ] Test scope creation and disposal
- [ ] Test nested scopes isolation
- [ ] Test context restoration after disposal

### CorrelationIdAccessor
- [ ] Test reading from CorrelationContext
- [ ] Test fallback to new GUID when no context

## Related Specs
- → [unit-testing.md](../../high-level-specs/unit-testing.md)
- → [correlation-id-flow.md](../../high-level-specs/correlation-id-flow.md)

---
## Notes
- UnitOfWorkExecutor is critical - handles transaction boundaries
- Max iteration limit (10) prevents infinite event loops
