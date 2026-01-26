# Task 01: Product Domain Refactoring

## Summary
Refactor Product domain to properly separate catalog concerns (Product) from inventory concerns (Stock). StockEntity becomes an independent Aggregate Root that owns StockReservationEntity as child entities. This aligns with DDD principles and prepares the domain for future scalability.

## Status
- [x] Completed

## Motivation

**Why separate aggregates?**

1. **Different bounded contexts** - Product Catalog vs Inventory Management have different stakeholders and use cases
2. **Different change frequency** - Stock changes often (reservations), Product changes rarely
3. **Independent queries** - "Show low stock items" shouldn't require loading Product catalog data
4. **Future scalability** - Inventory can become a separate microservice
5. **Reduced conflicts** - Separate aggregates = less concurrent modification conflicts

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
└── raises ProductCreatedDomainEvent on creation

StockEntity (Aggregate Root)
├── ProductId (1:1 relationship)
├── Quantity, LowStockThreshold
├── AvailableQuantity (computed), IsLowStock (computed)
├── ReserveStock(), ReleaseReservation(), ExpireReservation()
└── Reservations: List<StockReservationEntity>

StockReservationEntity (child of StockEntity)
├── StockId (FK to parent)
├── OrderId, ProductId, Quantity, Status, ExpiresAt
```

## Design Decisions

1. **Stock as separate aggregate** - Enables independent inventory queries and operations

2. **ProductCreatedDomainEvent** - When Product is created, domain event triggers Stock creation with initial quantity

3. **LowStockThreshold on StockEntity** - Inventory configuration belongs to inventory aggregate

4. **UpdateProductCommand** - Updates catalog fields AND LowStockThreshold (via StockEntity), but NOT StockQuantity
   - StockQuantity should only change through dedicated inventory operations (receiving goods, sales, etc.)

5. **StockId on StockReservationEntity** - Reservation is child of Stock aggregate, needs FK to parent

## Implementation Steps

### Phase 1: Domain Layer

**1.1 Create ProductCreatedDomainEvent**
- `src/Services/Products/Products.Domain/Events/ProductCreatedDomainEvent.cs`
- Contains: `ProductId`, `InitialStockQuantity`, `LowStockThreshold`

**1.2 Create StockEntity**
- `src/Services/Products/Products.Domain/Entities/StockEntity.cs`
- Extends `AggregateRoot`
- Properties: `ProductId`, `Quantity`, `LowStockThreshold`, `AvailableQuantity` (computed), `IsLowStock` (computed)
- Collection: `Reservations` with backing field
- Methods: `Create()`, `ReserveStock()`, `ReleaseReservation()`, `ExpireReservation()`, `UpdateLowStockThreshold()`

**1.3 Modify StockReservationEntity**
- Add `StockId` property (FK to parent aggregate)
- Change `Create()` to internal, accept `StockEntity` parent

**1.4 Modify ProductEntity**
- Remove: `StockQuantity`, `LowStockThreshold`, `IsLowStock`, `ReserveStock()`, `ReleaseStock()`
- Keep: `Name`, `Description`, `Price`, `Category`, `CreatedAt`, `UpdatedAt`
- Change `Create()` to raise `ProductCreatedDomainEvent`

### Phase 2: Application Layer

**2.1 Update IProductDbContext**
- Add: `DbSet<StockEntity> Stocks { get; }`

**2.2 Create ProductCreatedEventHandler**
- Handles `ProductCreatedDomainEvent`
- Creates `StockEntity` with initial quantity from event

**2.3 Update Command Handlers**
- `ReserveStockCommandHandler` - Query StockEntity, call `stock.ReserveStock()`
- `ReleaseStockCommandHandler` - Query StockEntity, call `stock.ReleaseReservation()`
- `ExpireReservationsCommandHandler` - Query StockEntity, call `stock.ExpireReservation()`
- `CreateProductCommandHandler` - Pass stock params to `ProductEntity.Create()` for domain event
- `UpdateProductCommandHandler` - Fetch both aggregates, update catalog on Product, LowStockThreshold on Stock (NOT StockQuantity)

**2.4 Update Query Handlers**
- Join with Stocks table to get StockQuantity, LowStockThreshold for API responses

**2.5 Update UpdateProductCommand**
- Remove `StockQuantity` parameter
- Keep `LowStockThreshold` parameter

### Phase 3: Infrastructure Layer

**3.1 Update ProductDbContext**
- Add: `public DbSet<StockEntity> Stocks => Set<StockEntity>();`

**3.2 Create StockConfiguration**
- Extends `AggregateRootConfiguration<StockEntity>`
- Unique index on `ProductId`
- Configure `HasMany(Reservations).WithOne().HasForeignKey(StockId)`

**3.3 Update StockReservationConfiguration**
- Add `StockId` property configuration
- Add index on `StockId`

**3.4 Update ProductConfiguration**
- Remove `StockQuantity`, `LowStockThreshold` configuration

### Phase 4: Migration

**4.1 Create EF Core Migration**
```bash
cd src/Services/Products/Products.Infrastructure
dotnet ef migrations add AddStockEntity --startup-project ../Products.API
```

**4.2 Data Migration SQL**
```sql
-- Create Stock records from existing Products
INSERT INTO "Stocks" ("Id", "ProductId", "Quantity", "LowStockThreshold", "Version", "CreatedAt", "UpdatedAt")
SELECT gen_random_uuid(), "Id", "StockQuantity", "LowStockThreshold", 0, NOW(), NOW()
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
| MODIFY | `Products.Application/Commands/UpdateProduct/UpdateProductCommand.cs` |
| MODIFY | `Products.Application/Commands/UpdateProduct/UpdateProductCommandValidator.cs` |
| MODIFY | `Products.Application/Commands/UpdateProduct/UpdateProductCommandHandler.cs` |
| MODIFY | `Products.Application/Commands/ReserveStock/ReserveStockCommandHandler.cs` |
| MODIFY | `Products.Application/Commands/ReleaseStock/ReleaseStockCommandHandler.cs` |
| MODIFY | `Products.Application/Commands/ExpireReservations/ExpireReservationsCommandHandler.cs` |
| MODIFY | `Products.Application/Commands/CreateProduct/CreateProductCommandHandler.cs` |
| MODIFY | `Products.Application/Queries/GetProducts/GetProductsQueryHandler.cs` |
| MODIFY | `Products.Application/Queries/GetProductById/GetProductByIdQueryHandler.cs` |
| MODIFY | `Products.Application/Queries/GetProductsBatch/GetProductsBatchQueryHandler.cs` |
| MODIFY | `Products.Infrastructure/Data/ProductDbContext.cs` |
| MODIFY | `Products.Infrastructure/Data/Configurations/ProductConfiguration.cs` |
| MODIFY | `Products.Infrastructure/Data/Configurations/StockReservationConfiguration.cs` |
| DELETE | `Products.Domain/Events/StockReleasedDomainEvent.cs` |
| DELETE | `Products.Domain/Events/StockReservationExpiredDomainEvent.cs` |
| DELETE | `Products.Domain/Events/StockReservedDomainEvent.cs` |

## Verification

1. **Build**: `dotnet build EShopDemo.sln`
2. **Tests**: `dotnet test EShopDemo.sln`
3. **Manual testing**:
   - Create product → verify Stock created via domain event
   - Reserve stock → verify reservation in StockEntity
   - Release stock → verify quantity restored
   - Update product → verify only LowStockThreshold changes on Stock (not Quantity)
   - API responses unchanged (StockQuantity still in response)

## Notes

- **API unchanged**: All existing endpoints return same data structure
- **Domain event flow**: `ProductEntity.Create()` → `ProductCreatedDomainEvent` → `ProductCreatedEventHandler` → `StockEntity.Create()`
- **Concurrency**: Both aggregates use `Version` from `AggregateRoot` for optimistic locking
- **UpdateProductCommand**: Changes catalog fields + LowStockThreshold, NOT StockQuantity
