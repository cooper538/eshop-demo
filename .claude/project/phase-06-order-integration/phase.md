# Phase 6: Order Service Integration

## Metadata
| Key | Value |
|-----|-------|
| Status | :white_circle: pending |

## Objective
Integrate Order Service with Product Service via dual-protocol communication

## Scope
- [ ] Implement `GrpcProductServiceClient` in ServiceClients
- [ ] Implement `HttpProductServiceClient` in ServiceClients
- [ ] Configure Polly resilience policies (retry, circuit breaker)
- [ ] Integrate stock reservation into CreateOrder flow
- [ ] Add CorrelationId propagation (middleware + interceptors)
- [ ] Write integration tests with mocked Product Service

## Related Specs
- → [dual-protocol-communication.md](../high-level-specs/dual-protocol-communication.md)
- → [grpc-communication.md](../high-level-specs/grpc-communication.md)
- → [correlation-id-flow.md](../high-level-specs/correlation-id-flow.md)

---
## Notes
(Updated during implementation)
