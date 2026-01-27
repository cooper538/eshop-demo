# Task 02: Circuit Breaker Policy

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | :white_circle: pending |
| Dependencies | task-01 |

## Summary
Add circuit breaker resilience policy to ResilienceInterceptor for gRPC calls.

## Scope
- [ ] Add Polly circuit breaker policy to ResilienceInterceptor
- [ ] Configure failure threshold (e.g., 5 failures)
- [ ] Configure break duration (e.g., 30 seconds)
- [ ] Configure sampling duration for failure counting
- [ ] Log circuit state changes (Closed -> Open -> HalfOpen)
- [ ] Combine with existing retry policy using PolicyWrap

## Reference Implementation
See `src/Common/EShop.ServiceClients/Interceptors/ResilienceInterceptor.cs` for existing retry logic.

## Related Specs
- [grpc-communication.md](../../high-level-specs/grpc-communication.md) (Section: Resilience Policies)

---
## Notes
(Updated during implementation)
