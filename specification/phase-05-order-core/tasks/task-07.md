# Task 07: External REST API

## Metadata
| Key | Value |
|-----|-------|
| ID | task-07 |
| Status | ✅ completed |
| Dependencies | task-04, task-05, task-06 |

## Summary
Create OrdersController with all external API endpoints and configure Program.cs with service setup.

## Scope

### OrdersController - IMPLEMENTED
- [x] `OrdersController` in Order.API/Controllers
  - Route: `api/[controller]`
  - XML documentation for OpenAPI generation

- [x] `GET /api/orders` - List orders with filtering
  - Query params: customerId (optional), page (default 1), pageSize (default 20)
  - Returns: `GetOrdersResult`

- [x] `GET /api/orders/{id}` - Get order by ID
  - Returns 200 OK with `OrderDto` or 404 Not Found

- [x] `POST /api/orders` - Create order
  - Body: `CreateOrderCommand`
  - Returns 201 Created with Location header

- [x] `POST /api/orders/{id}/cancel` - Cancel order
  - Body: `CancelOrderRequest` (contains Reason)
  - Returns 200 OK with `CancelOrderResult`

### Program.cs Configuration - IMPLEMENTED
- [x] YAML configuration loading via `AddYamlConfiguration("order")`
- [x] `AddServiceDefaults()` (Aspire)
- [x] `AddSerilog()` for logging
- [x] Health checks (PostgreSQL + Product Service)
- [x] Application layer registration
- [x] Infrastructure layer registration
- [x] Presentation layer registration
- [x] API defaults middleware
- [x] Order endpoints mapping
- [x] Default endpoints mapping

### Presentation Layer DI - IMPLEMENTED
- [x] `DependencyInjection.cs` with `AddPresentation()` extension
  - Options validation for `OrderSettings`
  - **Service clients registration** (Product Service gRPC client)
  - API defaults, controllers, OpenAPI

## Actual Implementation Structure
```
Order.API/
├── Configuration/
│   ├── OrderSettings.cs
│   └── order.settings.schema.json
├── Controllers/
│   └── OrdersController.cs
├── Properties/
│   └── launchSettings.json
├── DependencyInjection.cs
├── Order.API.csproj
├── Order.API.json
├── order.settings.yaml
├── order.settings.Development.yaml
├── order.settings.Production.yaml
└── Program.cs
```

## Key Implementation Details

### Health Checks
Includes health checks for:
- PostgreSQL database (`orderdb`)
- Product Service API (`products-api`)

### Service Clients (beyond original scope)
- Registers `IProductServiceClient` via `AddServiceClients()`
- Enables gRPC communication with Product Service

## Reference Implementation
See `ProductsController` and `Program.cs` in Products.API

## Related Specs
- -> [order-service-interface.md](../../high-level-specs/order-service-interface.md) (Section 2: HTTP Endpoints)
- -> [error-handling.md](../../high-level-specs/error-handling.md) (Section 5: Implementation)

---
## Notes
Service clients configured for Product Service integration.
