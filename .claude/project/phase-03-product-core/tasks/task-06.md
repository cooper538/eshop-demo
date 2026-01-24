# Task 06: FluentValidation

## Metadata
| Key | Value |
|-----|-------|
| ID | task-06 |
| Status | ⚪ pending |
| Dependencies | task-01, task-05 |

## Summary
Add FluentValidation validators for command validation.

## Scope
- [ ] Create CreateProductCommandValidator
  - Name: required, max 200 chars
  - Price: greater than 0
  - StockQuantity: >= 0
  - LowStockThreshold: >= 0
  - Category: max 100 chars
- [ ] Create UpdateProductCommandValidator (same rules + Id required)
- [ ] Register validators in DI
- [ ] Add ValidationBehavior pipeline (from EShop.Common)

## Related Specs
- → [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 2.2: Create Product - field constraints)
- → [error-handling.md](../../high-level-specs/error-handling.md) (Section 2.2: Validation Errors)

---
## Notes
(Updated during implementation)
