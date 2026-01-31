# Task 7: E2E Error Flow Validation

## Metadata
| Key | Value |
|-----|-------|
| ID | task-07 |
| Status | 🔵 in_progress |
| Dependencies | task-06 |
| Type | Validation (non-development) |

## Summary
Complete E2E validation of error handling flows across the microservices system. This is a verification task using `/e2e-test` skill.

## Scope

### HTTP Error Flows
- [ ] 404 Not Found (non-existent product in order)
- [ ] 404 Not Found (non-existent order lookup)
- [ ] 400 Bad Request (missing required fields)
- [ ] 400 Bad Request (invalid data types/formats)

### Business Rule Violations
- [ ] Duplicate order cancellation (already cancelled)
- [ ] Cancellation of rejected order
- [ ] Zero/negative quantity in order

### Service Communication Errors
- [ ] Product Service unavailable (gRPC error handling)

### Documentation
- [ ] Document test results in this file's Notes section

## How to Execute

1. Start services: `dotnet run --project src/AppHost`
2. Run validation: `/e2e-test unhappy`
3. For manual tests: Follow steps in spec file

## Related Specs
- [task-07-e2e-error-flow-validation-spec.md](./task-07-e2e-error-flow-validation-spec.md) (Detailed test scenarios)

## Tools
- `/e2e-test` skill - automated test runner
- `tools/e2e-test/` - shell scripts for manual testing
- Aspire Dashboard - logs and error traces

---
## Notes
(Updated during validation)
