# Task 05: CorrelationId Client Interceptor

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | 🔵 in_progress |
| Dependencies | task-01 |

## Summary
Register CorrelationIdClientInterceptor in gRPC client pipeline to propagate correlation IDs across service boundaries.

## Scope
- [ ] Implement CorrelationIdClientInterceptor in EShop.ServiceClients
- [ ] Read CorrelationId from current context (ICorrelationIdAccessor or AsyncLocal)
- [ ] Add CorrelationId to gRPC call metadata headers
- [ ] Register interceptor in gRPC client channel options
- [ ] Add integration test verifying CorrelationId propagation

## Reference Implementation
See `src/Common/EShop.Common/Correlation/` for CorrelationId infrastructure.

## Related Specs
- [correlation-id-flow.md](../../high-level-specs/correlation-id-flow.md) (Section: Client Interceptor)
- [grpc-communication.md](../../high-level-specs/grpc-communication.md) (Section: Interceptors)

---
## Notes
(Updated during implementation)
