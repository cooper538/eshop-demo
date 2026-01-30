# Task 07: External REST API

## Metadata
| Key | Value |
|-----|-------|
| ID | task-07 |
| Status | ✅ completed |
| Dependencies | task-01, task-04, task-05, task-06 |

## Summary
Create ProductsController with all external API endpoints.

## Scope
- [x] Create ProductsController in Product.API/Controllers
- [x] GET /api/products - list with filtering and pagination
- [x] GET /api/products/{id} - get by ID
- [x] POST /api/products - create product (return 201 Created with Location header)
- [x] PUT /api/products/{id} - update product
- [x] Configure Program.cs with MediatR, FluentValidation, EF Core, error handling
- [x] Add Swagger/OpenAPI configuration
- [x] Register service in AppHost

## Related Specs
- → [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 2: External API)
- → [error-handling.md](../../high-level-specs/error-handling.md) (Section 5: Implementation)

---
## Notes
(Updated during implementation)
