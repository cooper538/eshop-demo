# Task 7: E2E Error Flow Validation

## Metadata
| Key | Value |
|-----|-------|
| ID | task-07 |
| Status | ⚪ pending |
| Dependencies | task-06 |
| Type | Validation (non-dev) |

## Summary
Complete E2E validation of basic error flows across the microservices system. This is a verification task, not implementation.

## Scope

### HTTP Error Flows
- [ ] Validate 404 Not Found (non-existent product in order)
- [ ] Validate 404 Not Found (non-existent order lookup)
- [ ] Validate 400 Bad Request (missing required fields)
- [ ] Validate 400 Bad Request (invalid data types/formats)

### Business Rule Violations
- [ ] Validate duplicate order cancellation (already cancelled)
- [ ] Validate cancellation of rejected order
- [ ] Validate zero/negative quantity in order

### Service Communication Errors
- [ ] Validate error handling when Product Service is unavailable (gRPC)
- [ ] Validate timeout behavior for long-running operations

### Documentation
- [ ] Document test results and any issues found

## Related Specs
- → [task-07-e2e-error-flow-validation-spec.md](./task-07-e2e-error-flow-validation-spec.md) (Test scenarios and verification steps)

---
## Notes
(Updated during validation)
