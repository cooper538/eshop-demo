# Task 15: CorrelationId E2E Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-15 |
| Status | ⚪ pending |
| Dependencies | task-13 |

## Objective
Verify CorrelationId propagates correctly across all services and communication channels.

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

## Dependencies
- Depends on: task-13
- Blocks: none

## Acceptance Criteria
- [ ] Single CorrelationId traces through all services
- [ ] CorrelationId present in all log entries
- [ ] CorrelationId propagates via HTTP, gRPC, and messaging
- [ ] External request without CorrelationId gets one generated

## Notes
- May need to capture/inspect logs programmatically
- Consider using test ILogger that captures correlation
- Verify OpenTelemetry trace correlation if configured
- Check both request and response headers
