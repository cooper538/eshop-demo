# Task 3: Idempotent Consumer Base

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ✅ completed |
| Dependencies | task-02 |

## Summary
Create IdempotentConsumer<T> base class that implements inbox pattern for all consumers.

## Scope
- [ ] Create `IdempotentConsumer<TMessage>` abstract base class implementing `IConsumer<TMessage>`
- [ ] Implement message deduplication check using `InboxDbContext`
- [ ] Wrap processing in database transaction
- [ ] Record processed message after successful handling
- [ ] Add logging for duplicate message detection
- [ ] Define abstract `ProcessMessage(ConsumeContext<TMessage>)` method for derived classes

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section: 6.3. Idempotent Consumer Base Class)

---
## Notes
(Updated during implementation)
