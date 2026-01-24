# Phase 6: Order Service Integration

## Metadata
| Key | Value |
|-----|-------|
| Status | :white_circle: pending |

## Objective
Integrate Order Service with Product Service via gRPC communication

## Scope
- [ ] Configure gRPC client in ServiceClients for Order Service
- [ ] Configure Polly resilience policies (retry, circuit breaker)
- [ ] Integrate stock reservation into CreateOrder flow
- [ ] Add CorrelationId propagation (middleware + interceptors)

## Related Specs
- → [grpc-communication.md](../high-level-specs/grpc-communication.md)
- → [correlation-id-flow.md](../high-level-specs/correlation-id-flow.md)

---
## Notes
(Updated during implementation)
