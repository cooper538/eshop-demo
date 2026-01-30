# Task 08: CorrelationId E2E Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-08 |
| Status | ⚪ pending |
| Dependencies | task-06 |

## Summary
Verify CorrelationId propagates correctly across all services via HTTP, gRPC, and messaging.

## Scope
- [ ] Test CorrelationId generation at Gateway
  - [ ] Request without CorrelationId → Gateway generates one
  - [ ] Request with CorrelationId → Gateway preserves it
  - [ ] Response includes CorrelationId header
- [ ] Test CorrelationId in HTTP propagation
  - [ ] Gateway → Order Service (HTTP header)
  - [ ] Verify logged with CorrelationId
- [ ] Test CorrelationId in gRPC propagation
  - [ ] Order Service → Product Service (gRPC metadata)
  - [ ] Verify logged with CorrelationId
- [ ] Test CorrelationId in message propagation
  - [ ] Order Service → RabbitMQ → Notification Service
  - [ ] CorrelationId in message headers
  - [ ] Verify logged in consumer
- [ ] Test distributed tracing correlation
  - [ ] All services use same CorrelationId for single request
  - [ ] Logs can be correlated across services

## Related Specs
- → [correlation-id-flow.md](../../high-level-specs/correlation-id-flow.md)
- → [functional-testing.md](../../high-level-specs/functional-testing.md) (Section: E2E Testing)

---
## Notes
- May need to capture/inspect logs programmatically
- Consider using test ILogger that captures correlation
- Verify OpenTelemetry trace correlation if configured
- Check both request and response headers
