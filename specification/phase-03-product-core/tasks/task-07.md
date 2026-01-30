# Task 07: External REST API & Internal gRPC API

## Metadata
| Key | Value |
|-----|-------|
| ID | task-07 |
| Status | ✅ completed |
| Dependencies | task-01, task-04, task-05, task-06 |

## Summary
Create ProductsController for external REST API and ProductGrpcService for internal service communication.

## Scope
**REST API (ProductsController)**:
- [x] Create ProductsController in Products.API/Controllers
- [x] GET /api/products - list with filtering (category) and pagination (page, pageSize)
- [x] GET /api/products/{id} - get by ID (returns 404 if not found)
- [x] POST /api/products - create product (return 201 Created with Location header)
- [x] PUT /api/products/{id} - update product (validates route ID matches body)

**gRPC API (ProductGrpcService)**:
- [x] Create ProductGrpcService implementing ProductService.ProductServiceBase
- [x] GetProducts(productIds) - batch fetch products (atomic: fails if any not found)
- [x] ReserveStock(orderId, items) - reserve stock for order
- [x] ReleaseStock(orderId) - release stock reservation

**Configuration**:
- [x] Configure Program.cs with layered DI (Application, Infrastructure, Presentation)
- [x] Add Swagger/OpenAPI at /api/products endpoint
- [x] Configure gRPC with interceptors (CorrelationId, Logging, Validation, Exception)
- [x] Register service in AppHost

## Related Specs
- → [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 2: External API)
- → [grpc-communication.md](../../high-level-specs/grpc-communication.md)
- → [error-handling.md](../../high-level-specs/error-handling.md) (Section 5: Implementation)

---
## Notes
**Program.cs structure** (minimal, clean):
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddYamlConfiguration("product");
builder.AddServiceDefaults();
builder.AddSerilog();

builder.Services.AddHealthChecks().AddPostgresHealthCheck("productdb");

builder.Services.AddApplication();
builder.AddInfrastructure();
builder.AddPresentation();

var app = builder.Build();

app.UseApiDefaults();
app.MapProductsEndpoints();
app.MapDefaultEndpoints();

app.Run();
```

**gRPC interceptor stack**:
1. `CorrelationIdServerInterceptor` - propagates correlation ID
2. `GrpcLoggingInterceptor` - logs requests/responses
3. `GrpcValidationInterceptor` - validates requests
4. `GrpcExceptionInterceptor` - converts exceptions to gRPC status codes

**gRPC batch GetProducts** follows Google AIP-231: atomic operation, fails if any product not found
