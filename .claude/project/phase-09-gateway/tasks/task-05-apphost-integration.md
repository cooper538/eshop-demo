# Task 5: AppHost Integration

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | ✅ completed |
| Dependencies | task-01, task-02, task-03, task-04 |

## Summary
Register Gateway in Aspire AppHost with service references.

## Scope
- [x] Add Gateway project reference to AppHost
- [x] Register Gateway service with `AddProject<>()`
- [x] Add references to Product and Order services for service discovery
- [x] Configure Gateway as external endpoint (expose to outside)
- [x] Verify Gateway appears in Aspire dashboard
- [x] Test end-to-end flow through Gateway → Services

## Related Specs
- → [aspire-orchestration.md](../../high-level-specs/aspire-orchestration.md)

---
## Notes
- Gateway registered with `WithExternalHttpEndpoints()` to expose as single entry point
- Service discovery enabled via `WithReference(productService)` and `WithReference(orderService)`
