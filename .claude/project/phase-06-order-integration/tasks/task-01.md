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
- [ ] Add project reference to EShop.ServiceClients in Order.API.csproj
- [ ] Register ServiceClients in Order.API Program.cs using AddServiceClients()
- [ ] Configure ProductServiceClient with proper service discovery URI
- [ ] Verify IProductServiceClient is injectable in handlers

## Reference Implementation
See `src/Services/Products/Products.API/Program.cs` for service registration patterns.

## Related Specs
- [grpc-communication.md](../../high-level-specs/grpc-communication.md) (Section: Client Configuration)

---
## Notes
(Updated during implementation)
