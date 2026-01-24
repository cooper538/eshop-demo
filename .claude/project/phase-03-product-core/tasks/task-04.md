# Task 04: CQRS Queries

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | 🔵 in_progress |
| Dependencies | task-01, task-02, task-03 |

## Summary
Implement query handlers for reading product data.

## Scope
- [ ] Create GetProductsQuery with filtering (category) and pagination (page, pageSize)
- [ ] Create GetProductsQueryHandler using IProductDbContext
- [ ] Create GetProductsResult DTO with Items, Page, PageSize, TotalCount, TotalPages
- [ ] Create GetProductByIdQuery(Guid id)
- [ ] Create GetProductByIdQueryHandler
- [ ] Throw NotFoundException when product not found
- [ ] Create ProductDto for response mapping

## Related Specs
- → [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 2.1: List Products)
- → [error-handling.md](../../high-level-specs/error-handling.md) (Section 6.1: Throwing Exceptions)

---
## Notes
(Updated during implementation)
