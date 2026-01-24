# Task 05: CQRS Commands

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | ⚪ pending |
| Dependencies | task-01, task-02, task-03 |

## Summary
Implement command handlers for creating and updating products.

## Scope
- [ ] Create CreateProductCommand with Name, Description, Price, StockQuantity, LowStockThreshold, Category
- [ ] Create CreateProductCommandHandler
- [ ] Return created ProductDto with generated Id
- [ ] Create UpdateProductCommand with Id, Name, Description, Price, StockQuantity, LowStockThreshold, Category
- [ ] Create UpdateProductCommandHandler
- [ ] Throw NotFoundException when product not found
- [ ] Handle optimistic concurrency conflicts (ConflictException)

## Related Specs
- → [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 2.2: Create Product)
- → [error-handling.md](../../high-level-specs/error-handling.md) (Section 3: Exception Hierarchy)

---
## Notes
(Updated during implementation)
