# Task 04: CQRS Queries

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | ✅ completed |
| Dependencies | task-01, task-02, task-03 |

## Summary
Implement query handlers for reading product data.

## Scope
- [x] Create GetProductsQuery with filtering (category) and pagination (page, pageSize)
- [x] Create GetProductsQueryHandler using IProductDbContext
- [x] Create GetProductsResult DTO with Items, Page, PageSize, TotalCount, TotalPages
- [x] Create GetProductByIdQuery(Guid id)
- [x] Create GetProductByIdQueryHandler
- [x] Throw NotFoundException when product not found
- [x] Create ProductDto for response mapping

## Related Specs
- → [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 2.1: List Products)
- → [error-handling.md](../../high-level-specs/error-handling.md) (Section 6.1: Throwing Exceptions)

---
## Notes
(Updated during implementation)
