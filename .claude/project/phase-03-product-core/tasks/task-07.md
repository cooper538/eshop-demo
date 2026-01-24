# Task 07: External REST API

## Metadata
| Key | Value |
|-----|-------|
| ID | task-07 |
| Status | ⚪ pending |
| Dependencies | task-01, task-04, task-05, task-06 |

## Summary
Create ProductsController with all external API endpoints.

## Scope
- [ ] Create ProductsController in Product.API/Controllers
- [ ] GET /api/products - list with filtering and pagination
- [ ] GET /api/products/{id} - get by ID
- [ ] POST /api/products - create product (return 201 Created with Location header)
- [ ] PUT /api/products/{id} - update product
- [ ] Configure Program.cs with MediatR, FluentValidation, EF Core, error handling
- [ ] Add Swagger/OpenAPI configuration
- [ ] Register service in AppHost

## Related Specs
- → [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 2: External API)
- → [error-handling.md](../../high-level-specs/error-handling.md) (Section 5: Implementation)

---
## Notes
(Updated during implementation)
