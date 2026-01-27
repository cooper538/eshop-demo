# Task 09: Notification Consumers Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-09 |
| Status | ⚪ pending |
| Dependencies | task-08 |

## Objective
Unit tests for Notification Service event consumers and idempotent processing.

## Scope
- [ ] Test `IdempotentConsumer<T>` base class
  - [ ] First message processed successfully
  - [ ] Duplicate message (same MessageId) skipped
  - [ ] Different message processed
- [ ] Test `OrderConfirmedConsumer`
  - [ ] Valid event → email sent
  - [ ] Email service failure → proper error handling
- [ ] Test `OrderRejectedConsumer`
  - [ ] Valid event → email sent with rejection reason
- [ ] Test `OrderCancelledConsumer`
  - [ ] Valid event → email sent
- [ ] Test `StockLowConsumer`
  - [ ] Low stock event → notification sent
- [ ] Test `EmailService`
  - [ ] Successful send
  - [ ] Failure handling (mock HTTP client)

## Dependencies
- Depends on: task-08
- Blocks: none

## Acceptance Criteria
- [ ] Idempotent consumer prevents duplicate processing
- [ ] All event consumers tested for happy path
- [ ] Email service error handling tested

## Notes
- Use MassTransit ITestHarness to publish test events
- Mock IEmailService in consumer tests
- For EmailService tests, mock HttpClient or use WireMock
- Verify Inbox pattern records processed messages
