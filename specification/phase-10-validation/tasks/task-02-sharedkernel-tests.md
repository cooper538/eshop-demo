# Task 02: SharedKernel Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Summary
Unit tests for EShop.SharedKernel DDD building blocks - foundational classes used across all services.

## Scope

### Entity Base Class
- [ ] Test equality based on Id
- [ ] Test hash code consistency

### AggregateRoot
- [ ] Test domain event collection (add, clear)
- [ ] Test version tracking

### ValueObject
- [ ] Test equality based on components
- [ ] Test equality operators (==, !=)
- [ ] Test hash code consistency

### Guard Utility
- [ ] Test null/empty string guards
- [ ] Test numeric range guards (negative, zero)

## Related Specs
- → [unit-testing.md](../../high-level-specs/unit-testing.md)

---
## Notes
- Create test entity/value object classes for testing (TestEntity, TestValueObject)
- These tests validate DDD infrastructure used by all services
