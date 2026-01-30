# Task 01: Register ServiceClients in Order Service

## Metadata
| Key | Value |
|-----|-------|
| ID | task-01 |
| Status | ✅ completed |
| Dependencies | - |

## Summary
Configure EShop.ServiceClients in Order.API to enable gRPC communication with Product Service.

## Scope
- [x] Add project reference to EShop.ServiceClients in Order.API.csproj
- [x] Register ServiceClients in Order.API DependencyInjection using AddServiceClients()
- [x] Configure ProductServiceClient with Aspire service discovery
- [x] Verify IProductServiceClient is injectable in command handlers

## Implementation

### Registration
```csharp
// Order.API/DependencyInjection.cs
builder.Services.AddServiceClients(builder.Configuration, builder.Environment);
```

### Service Discovery
Uses Aspire's `AddServiceDiscovery()` with `https+http://products-api` endpoint configured via ServiceClientOptions.

### Key Files
- `src/Services/Order/Order.API/DependencyInjection.cs` (line 17)
- `src/Common/EShop.ServiceClients/Extensions/ServiceCollectionExtensions.cs`

## Related Specs
- [grpc-communication.md](../../high-level-specs/grpc-communication.md) (Section: Client Configuration)

---
## Notes
ServiceClients are registered in AddPresentation() which is called from Program.cs.
