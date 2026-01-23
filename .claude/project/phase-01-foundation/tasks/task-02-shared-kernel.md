# Task 2: EShop.SharedKernel

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Summary
Implementovat čisté DDD building blocks bez externích závislostí.

## Scope
- [ ] Vytvořit projekt `EShop.SharedKernel` v `src/Common/EShop.SharedKernel/`
- [ ] Implementovat `Domain/Entity.cs` - base entity s Id a domain events kolekcí
- [ ] Implementovat `Domain/AggregateRoot.cs` - aggregate root s verzí
- [ ] Implementovat `Domain/IAggregateRoot.cs` - marker interface
- [ ] Implementovat `Domain/ValueObject.cs` - value object s equality
- [ ] Implementovat `Events/IDomainEvent.cs` - domain event interface
- [ ] Implementovat `Events/DomainEventBase.cs` - base record s timestamp
- [ ] Implementovat `Guards/Guard.cs` - guard clauses (Against.Null, NullOrEmpty, NegativeOrZero, Negative)

## Related Specs
- → [shared-projects.md](../../high-level-specs/shared-projects.md) (Section: 3.1 - EShop.SharedKernel)

---
## Notes
(Updated during implementation)
