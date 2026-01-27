# Phase 6: Order Service Integration

## Metadata
| Key | Value |
|-----|-------|
| Status | ✅ completed |

## Objective
Integrate Order Service with Product Service via gRPC communication

## Scope
- [x] Configure gRPC client in ServiceClients for Order Service
- [x] Configure Polly resilience policies (retry, circuit breaker)
- [x] Integrate stock reservation into CreateOrder flow
- [x] Add CorrelationId propagation (middleware + interceptors)

## Tasks

| # | ID | Task | Dependencies |
|---|-----|------|--------------|
| 1 | task-01 | Register ServiceClients in Order Service | - |
| 2 | task-02 | Circuit Breaker Policy | task-01 |
| 3 | task-03 | Stock Reservation Integration | task-01 |
| 4 | task-04 | Stock Release Integration | task-03 |
| 5 | task-05 | CorrelationId Client Interceptor | task-01 |

## Related Specs
- [grpc-communication.md](../high-level-specs/grpc-communication.md)
- [correlation-id-flow.md](../high-level-specs/correlation-id-flow.md)

---
## Notes
(Updated during implementation)
