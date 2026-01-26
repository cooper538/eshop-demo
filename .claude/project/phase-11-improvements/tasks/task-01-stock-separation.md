# Task 01: Stock Separation - Refactor Product Domain

## Summary
Refactor Product domain to separate catalog concerns (Product) from inventory concerns (Stock). Create new `StockEntity` as independent Aggregate Root that owns `StockReservationEntity` as child entities.

## Status
- [ ] Not started

## Architecture Change

**Before:**
```
ProductEntity (Aggregate Root)
├── Name, Description, Price, Category (catalog)
├── StockQuantity, LowStockThreshold (inventory) ❌ mixed concerns
└── ReserveStock(), ReleaseStock()

StockReservationEntity (standalone entity)
```

**After:**
```
ProductEntity (Aggregate Root)
├── Name, Description, Price, Category (catalog only)
└── raises ProductCreatedDomainEvent

StockEntity (Aggregate Root) - "skladová karta"
├── ProductId (1:1)
├── Quantity, LowStockThreshold
├── ReserveStock(), ReleaseReservation(), ExpireReservation()
└── Reservations: List<StockReservationEntity>

StockReservationEntity (child of StockEntity)
├── StockId (FK)
├── OrderId, ProductId, Quantity, Status, ExpiresAt
```

## Implementation Steps

### Phase 1: Domain Layer

**1.1 Create domain events**
- `src/Services/Products/Products.Domain/Events/ProductCreatedDomainEvent.cs`
  ```csharp
  public sealed record ProductCreatedDomainEvent(
      Guid ProductId, int InitialStockQuantity, int LowStockThreshold
  ) : DomainEventBase;
  ```

**1.2 Create StockEntity (new file)**
- `src/Services/Products/Products.Domain/Entities/StockEntity.cs`
- Properties: `ProductId`, `Quantity`, `LowStockThreshold`, `AvailableQuantity` (computed), `IsLowStock` (computed)
- Collection: `_reservations` with `IReadOnlyCollection<StockReservationEntity> Reservations`
- Methods:
  - `Create(productId, initialQuantity, lowStockThreshold)`
  - `ReserveStock(orderId, quantity)` → returns `StockReservationEntity`
  - `ReleaseReservation(orderId)`
  - `ExpireReservation(reservationId)`
  - `UpdateQuantity(newQuantity)`
  - `UpdateLowStockThreshold(threshold)`

**1.3 Modify StockReservationEntity**
- `src/Services/Products/Products.Domain/Entities/StockReservationEntity.cs`
- ADD: `StockId` property (FK to parent)
- CHANGE: `Create()` method to `internal` with `StockEntity stock` parameter
- Keep: `Release()`, `Expire()` methods unchanged

**1.4 Modify ProductEntity**
- `src/Services/Products/Products.Domain/Entities/ProductEntity.cs`
- REMOVE: `StockQuantity`, `LowStockThreshold`, `IsLowStock`
- REMOVE: `ReserveStock()`, `ReleaseStock()` methods
- KEEP: `Name`, `Description`, `Price`, `Category`, `CreatedAt`, `UpdatedAt`
- CHANGE: `Create()` to raise `ProductCreatedDomainEvent`
- CHANGE: `Update()` to only update catalog fields (remove stock params)

### Phase 2: Application Layer

**2.1 Update IProductDbContext**
- `src/Services/Products/Products.Application/Data/IProductDbContext.cs`
- ADD: `DbSet<StockEntity> Stocks { get; }`

**2.2 Create ProductCreatedEventHandler (new file)**
- `src/Services/Products/Products.Application/EventHandlers/ProductCreatedEventHandler.cs`
- Handles `ProductCreatedDomainEvent`
- Creates `StockEntity` with initial quantity (100 for testing)
- Saves to database

**2.3 Update ReserveStockCommandHandler**
- `src/Services/Products/Products.Application/Commands/ReserveStock/ReserveStockCommandHandler.cs`
- Query `StockEntity` instead of `ProductEntity`
- Include active reservations
- Call `stock.ReserveStock(orderId, quantity)`

**2.4 Update ReleaseStockCommandHandler**
- `src/Services/Products/Products.Application/Commands/ReleaseStock/ReleaseStockCommandHandler.cs`
- Query `StockEntity` with reservations for OrderId
- Call `stock.ReleaseReservation(orderId)`

**2.5 Update ExpireReservationsCommandHandler**
- `src/Services/Products/Products.Application/Commands/ExpireReservations/ExpireReservationsCommandHandler.cs`
- Query `StockEntity` with expired reservations
- Call `stock.ExpireReservation(reservationId)`

**2.6 Update CreateProductCommandHandler**
- `src/Services/Products/Products.Application/Commands/CreateProduct/CreateProductCommandHandler.cs`
- Keep accepting `StockQuantity`, `LowStockThreshold` in command (API compat)
- Pass to `ProductEntity.Create()` for domain event

**2.7 Update UpdateProductCommandHandler**
- `src/Services/Products/Products.Application/Commands/UpdateProduct/UpdateProductCommandHandler.cs`
- Fetch both `ProductEntity` and `StockEntity`
- Update catalog on Product, stock on Stock

**2.8 Update Query Handlers**
- `GetProductsQueryHandler.cs` - join with Stocks for StockQuantity
- `GetProductByIdQueryHandler.cs` - join with Stocks
- `GetProductsBatchQueryHandler.cs` - join with Stocks

### Phase 3: Infrastructure Layer

**3.1 Update ProductDbContext**
- `src/Services/Products/Products.Infrastructure/Data/ProductDbContext.cs`
- ADD: `public DbSet<StockEntity> Stocks => Set<StockEntity>();`

**3.2 Create StockConfiguration (new file)**
- `src/Services/Products/Products.Infrastructure/Data/Configurations/StockConfiguration.cs`
- Extends `AggregateRootConfiguration<StockEntity>`
- Unique index on `ProductId`
- Configure `HasMany(Reservations).WithOne().HasForeignKey(StockId)`

**3.3 Update StockReservationConfiguration**
- `src/Services/Products/Products.Infrastructure/Data/Configurations/StockReservationConfiguration.cs`
- ADD: `StockId` property configuration
- ADD: Index on `StockId`

**3.4 Update ProductConfiguration**
- `src/Services/Products/Products.Infrastructure/Data/Configurations/ProductConfiguration.cs`
- REMOVE: `StockQuantity`, `LowStockThreshold` configuration

### Phase 4: Migration

**4.1 Create EF Core Migration**
```bash
cd src/Services/Products/Products.Infrastructure
dotnet ef migrations add AddStockEntity --startup-project ../Products.API
```

**4.2 Migration SQL (data migration)**
```sql
-- Create Stock records from existing Products
INSERT INTO "Stocks" ("Id", "ProductId", "Quantity", "LowStockThreshold")
SELECT gen_random_uuid(), "Id", "StockQuantity", "LowStockThreshold"
FROM "Products";

-- Link existing reservations to stocks
UPDATE "StockReservations" sr
SET "StockId" = s."Id"
FROM "Stocks" s
WHERE sr."ProductId" = s."ProductId";
```

## Files Summary

| Action | File |
|--------|------|
| CREATE | `Products.Domain/Events/ProductCreatedDomainEvent.cs` |
| CREATE | `Products.Domain/Entities/StockEntity.cs` |
| CREATE | `Products.Application/EventHandlers/ProductCreatedEventHandler.cs` |
| CREATE | `Products.Infrastructure/Data/Configurations/StockConfiguration.cs` |
| MODIFY | `Products.Domain/Entities/ProductEntity.cs` |
| MODIFY | `Products.Domain/Entities/StockReservationEntity.cs` |
| MODIFY | `Products.Application/Data/IProductDbContext.cs` |
| MODIFY | `Products.Application/Commands/ReserveStock/ReserveStockCommandHandler.cs` |
| MODIFY | `Products.Application/Commands/ReleaseStock/ReleaseStockCommandHandler.cs` |
| MODIFY | `Products.Application/Commands/ExpireReservations/ExpireReservationsCommandHandler.cs` |
| MODIFY | `Products.Application/Commands/CreateProduct/CreateProductCommandHandler.cs` |
| MODIFY | `Products.Application/Commands/UpdateProduct/UpdateProductCommandHandler.cs` |
| MODIFY | `Products.Application/Queries/GetProducts/GetProductsQueryHandler.cs` |
| MODIFY | `Products.Application/Queries/GetProductById/GetProductByIdQueryHandler.cs` |
| MODIFY | `Products.Application/Queries/GetProductsBatch/GetProductsBatchQueryHandler.cs` |
| MODIFY | `Products.Infrastructure/Data/ProductDbContext.cs` |
| MODIFY | `Products.Infrastructure/Data/Configurations/StockReservationConfiguration.cs` |
| MODIFY | `Products.Infrastructure/Data/Configurations/ProductConfiguration.cs` |

## Verification

1. **Build**: `dotnet build EShopDemo.sln`
2. **Tests**: `dotnet test EShopDemo.sln`
3. **Manual testing**:
   - Create product → verify Stock created with 100 units
   - Reserve stock → verify reservation in StockEntity
   - Release stock → verify quantity restored
   - API responses unchanged (StockQuantity still in response)

## Notes

- **API unchanged**: All existing endpoints return same data structure
- **Domain event flow**: `ProductEntity.Create()` → `ProductCreatedDomainEvent` → `ProductCreatedEventHandler` → `StockEntity.Create()`
- **Concurrency**: `StockEntity` uses `Version` from `AggregateRoot` for optimistic locking
- **Initial stock**: 100 units for testing (configurable in handler)
