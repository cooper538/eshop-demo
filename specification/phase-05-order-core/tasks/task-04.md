# Task 04: CQRS Queries

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | ✅ completed |
| Dependencies | task-03 |

## Summary
Implement query handlers for reading order data with customer filtering and pagination.

## Scope

### DTOs - IMPLEMENTED
- [x] `OrderDto` (sealed record) in Application/Dtos
  - Properties: Id, CustomerId, CustomerEmail, Status, TotalAmount, RejectionReason, CreatedAt, UpdatedAt, Items
  - Static factory: `FromEntity(OrderEntity entity)`
- [x] `OrderItemDto` (sealed record)
  - Properties: ProductId, ProductName, Quantity, UnitPrice, LineTotal

### GetOrderByIdQuery - IMPLEMENTED
- [x] `GetOrderByIdQuery(Guid Id)` : IQuery<OrderDto>
- [x] `GetOrderByIdQueryHandler`
  - Fetches order by ID
  - Throws `NotFoundException` if not found
  - Returns `OrderDto.FromEntity(order)`
- [x] `GetOrderByIdQueryValidator`
  - Id: NotEmpty

### GetOrdersQuery (with filtering) - IMPLEMENTED
- [x] `GetOrdersQuery` : IQuery<GetOrdersResult>
  - Properties: CustomerId (optional filter), Page (default 1), PageSize (default 20)
- [x] `GetOrdersResult`
  - Properties: Items, Page, PageSize, TotalCount, TotalPages
- [x] `GetOrdersQueryHandler`
  - Applies CustomerId filter if provided
  - Orders by CreatedAt descending (newest first)
  - Applies pagination
- [x] `GetOrdersQueryValidator`
  - Page: GreaterThan(0)
  - PageSize: InclusiveBetween(1, 100)

## Actual Implementation Structure
```
Order.Application/
├── Dtos/
│   ├── OrderDto.cs
│   └── OrderItemDto.cs
└── Queries/
    ├── GetOrderById/
    │   ├── GetOrderByIdQuery.cs
    │   ├── GetOrderByIdQueryHandler.cs
    │   └── GetOrderByIdQueryValidator.cs
    └── GetOrders/
        ├── GetOrdersQuery.cs
        ├── GetOrdersQueryHandler.cs
        ├── GetOrdersQueryValidator.cs
        └── GetOrdersResult.cs
```

## Reference Implementation
See `GetProductsQuery` and `GetProductByIdQuery` in Products.Application

## Related Specs
- -> [order-service-interface.md](../../high-level-specs/order-service-interface.md) (Section 2: HTTP Endpoints)

---
## Notes
(Updated during implementation)
