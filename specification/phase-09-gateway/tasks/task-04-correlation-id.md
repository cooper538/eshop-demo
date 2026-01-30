# Task 4: CorrelationId Integration

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | ✅ completed |
| Dependencies | task-02 |

## Summary
Integrate existing CorrelationIdMiddleware for request tracking on external requests.

## Scope
- [x] Add CorrelationId services and middleware from EShop.Common.Api
- [x] Ensure CorrelationId is generated for requests without one (external requests)
- [x] Ensure CorrelationId is forwarded to downstream services via YARP
- [x] Verify CorrelationId appears in response headers
- [x] Test CorrelationId propagation through gateway to services

## Related Specs
- → [correlation-id-flow.md](../../high-level-specs/correlation-id-flow.md)

---
## Notes
- CorrelationIdMiddleware was added in task-01 as part of project setup
- YARP automatically forwards X-Correlation-ID header to downstream services
