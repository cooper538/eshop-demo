# Code Guidelines

Project-specific C# standards.

## Contents

1. [DDD](#ddd)
2. [Mapping](#mapping)
3. [Naming](#naming)
4. [File Organization](#file-organization)
5. [Class Structure](#class-structure)
6. [Documentation](#documentation)
7. [CancellationToken](#cancellationtoken)
8. [Forbidden](#forbidden)

---

## DDD

| Element | Suffix | Example |
|---------|--------|---------|
| Entity | `Entity` | `ProductEntity`, `OrderItemEntity` |
| Value Object | `VO` | `AddressVO`, `MoneyVO`, `EmailVO` |
| Domain Event | `Event` | `OrderCreatedEvent`, `StockReservedEvent` |
| Domain Service | `Service` | `PricingService`, `InventoryService` |

All domain types live only in `Domain/` layer.

---

## Mapping

Mapper lives on the type in **higher layer** (the one that knows the target type):
- `Command.ToEntity()` - Application → Domain
- `Dto.FromEntity(entity)` - Application ← Domain
- Entity never has mapping methods (Domain doesn't know Application)

---

## Naming

| Element | Convention | Example |
|---------|------------|---------|
| Enums | prefix `E`, suffix `Type` | `EOrderStateType` |

- Include units in variable names: `pollIntervalMs`, `fileSizeGb`
- Always use `var`

---

## File Organization

- **1 file = 1 type** (exception: nested private classes)
- **Enums always in separate file**
- Generic file naming: `Something.cs` (non-generic), `Something(T).cs` (generic)
- Group interfaces in `Interfaces/` folder if needed
- Group enums in `Enumerations/` folder if needed

---

## Class Structure

```
1. Constant Fields
2. Fields
3. Constructors
4. Properties
5. Public Methods
6. Private Methods
```

---

## Documentation

**Controller methods** - XML docs for Swagger:
```csharp
/// <summary>Retrieves a product by its ID.</summary>
/// <param name="id">The ID of the product</param>
/// <returns>The product DTO</returns>
public async Task<ActionResult<ProductResponseDto>> GetProduct(string id)
```

**TODO comments** - include ticket number:
```csharp
// TODO: Will be implemented in TASK XX-XX short description
```

---

## CancellationToken

**Controllers:**
- ✅ **GET requests** - use `CancellationToken ct = default` (saves resources on cancelled requests)
- ❌ **POST/PUT/DELETE** - do NOT use (risk of data inconsistency if cancelled mid-operation)
- **XML docs:** Do not document `CancellationToken` parameter (internal implementation detail).

---

## Forbidden

- `dynamic` keyword (except in tests)
- Nested ternary operators
- Magic numbers
