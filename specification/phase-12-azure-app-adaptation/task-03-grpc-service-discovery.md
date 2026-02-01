# Task 03: gRPC Service Discovery

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ✅ completed |
| Dependencies | - |

## Summary
Configure gRPC clients for Azure Container Apps service discovery using environment-aware configuration.

## Scope
- [x] Update `ServiceCollectionExtensions.cs` to conditionally skip Aspire service discovery in Production
- [x] Use `IsDevelopment()` for Aspire service discovery with dev certificate bypass
- [x] Use `IsProduction()` for Azure with SocketsHttpHandler (HTTP/2 multiplexing)
- [x] Service URLs injected via environment variables in Container Apps

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 8.2 Service Discovery)
- -> [internal-api-communication.md](../high-level-specs/internal-api-communication.md) (gRPC configuration)

---
## Notes
- No new extension class needed - modified existing `EShop.ServiceClients/Extensions/ServiceCollectionExtensions.cs`
- Service URLs: `ServiceClients__ProductService__Url` env var -> `ServiceClients:ProductService:Url` config
