# Task 08: Unit Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-08 |
| Status | ⚪ pending |
| Dependencies | task-02, task-04, task-05 |

## Summary
Write unit tests for domain entities and MediatR handlers.

## Scope
- [ ] Create Product.UnitTests project
- [ ] Add xUnit, Moq, FluentAssertions packages
- [ ] Create ProductTests (Domain)
  - ReserveStock_WhenSufficientQuantity_DecreasesStockAndReturnsTrue
  - ReserveStock_WhenInsufficientQuantity_ReturnsFalseAndDoesNotChange
  - IsLowStock_WhenBelowThreshold_ReturnsTrue
- [ ] Create TestDbContextFactory helper (InMemory DbContext)
- [ ] Create GetProductByIdQueryHandlerTests
  - Handle_WhenProductExists_ReturnsProductDto
  - Handle_WhenProductNotFound_ThrowsNotFoundException
- [ ] Create CreateProductCommandHandlerTests
  - Handle_WithValidCommand_CreatesProductAndReturnsDto

## Related Specs
- → [unit-testing.md](../../high-level-specs/unit-testing.md) (Sections 5-6: Domain and Handler Testing)

---
## Notes
(Updated during implementation)
