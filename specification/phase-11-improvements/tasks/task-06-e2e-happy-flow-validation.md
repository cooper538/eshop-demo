# Task 6: E2E Happy Flow Validation

## Metadata
| Key | Value |
|-----|-------|
| ID | task-06 |
| Status | ✅ completed |
| Dependencies | task-05 |
| Type | Validation (non-development) |

## Summary
Complete E2E validation of all happy flows across the microservices system. This is a verification task using `/e2e-test` skill.

## Scope

### Order Flows
- [ ] Create Order flow (stock reserved -> confirmed -> notification)
- [ ] Cancel Order flow (cancelled -> stock released -> notification)
- [ ] Order Rejection flow (insufficient stock -> rejected -> notification)

### Stock & Notification Flows
- [ ] Stock Low Alert flow (stock below threshold -> notification)

### Infrastructure Flows
- [ ] Correlation ID propagation (Gateway -> Order -> Product -> Notification)

### Documentation
- [ ] Document test results in this file's Notes section

## How to Execute

1. Start services: `dotnet run --project src/AppHost`
2. Run validation: `/e2e-test happy`
3. For specific flows: `/e2e-test cancel`, `/e2e-test trace <correlation-id>`
4. For manual tests: Follow steps in spec file

## Related Specs
- [task-06-e2e-happy-flow-validation-spec.md](./task-06-e2e-happy-flow-validation-spec.md) (Detailed test scenarios)

## Tools
- `/e2e-test` skill - automated test runner
- `tools/e2e-test/` - shell scripts for manual testing
- Aspire Dashboard - logs and traces

---
## Notes
(Updated during validation)
