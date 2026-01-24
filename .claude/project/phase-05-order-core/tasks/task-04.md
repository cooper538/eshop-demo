# Task 04: CQRS Queries

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | :white_circle: pending |
| Dependencies | task-03 |

## Summary
Implement query handlers for reading order data with customer filtering.

## Scope

### DTOs
- [ ] Create `OrderDto` (sealed record) in Application/Dtos
  ```csharp
  public sealed record OrderDto(
      Guid Id,
      Guid CustomerId,
      string CustomerEmail,
      string Status,
      decimal TotalAmount,
      string? RejectionReason,
      DateTime CreatedAt,
      DateTime? UpdatedAt,
      IReadOnlyList<OrderItemDto> Items
  )
  {
      public static OrderDto FromEntity(OrderEntity entity) => ...;
  }
  ```
- [ ] Create `OrderItemDto` (sealed record)
  ```csharp
  public sealed record OrderItemDto(
      Guid ProductId,
      string ProductName,
      int Quantity,
      decimal UnitPrice,
      decimal LineTotal
  );
  ```

### GetOrderByIdQuery
- [ ] Create `GetOrderByIdQuery(Guid Id)` : IQuery<OrderDto>
- [ ] Create `GetOrderByIdQueryHandler`
  - Fetch order by ID
  - Throw `NotFoundException` if not found
  - Return `OrderDto.FromEntity(order)`
- [ ] Create `GetOrderByIdQueryValidator`
  - Id: NotEmpty

### GetOrdersQuery (with filtering)
- [ ] Create `GetOrdersQuery` : IQuery<GetOrdersResult>
  ```csharp
  public sealed record GetOrdersQuery(
      Guid? CustomerId,  // Optional filter
      int Page = 1,
      int PageSize = 20
  ) : IQuery<GetOrdersResult>;
  ```
- [ ] Create `GetOrdersResult`
  ```csharp
  public sealed record GetOrdersResult(
      IReadOnlyList<OrderDto> Items,
      int Page,
      int PageSize,
      int TotalCount,
      int TotalPages
  );
  ```
- [ ] Create `GetOrdersQueryHandler`
  - Apply CustomerId filter if provided
  - Order by CreatedAt descending (newest first)
  - Apply pagination
- [ ] Create `GetOrdersQueryValidator`
  - Page: GreaterThan(0)
  - PageSize: InclusiveBetween(1, 100)

## Reference Implementation
See `GetProductsQuery` and `GetProductByIdQuery` in Products.Application

## Related Specs
- → [order-service-interface.md](../../high-level-specs/order-service-interface.md) (Section 2: HTTP Endpoints)

---
## Notes
(Updated during implementation)
