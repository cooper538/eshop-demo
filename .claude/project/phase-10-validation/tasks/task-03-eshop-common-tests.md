# Task 03: EShop.Common Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ⚪ pending |
| Dependencies | task-01 |

## Objective
Unit tests for shared behaviors, middleware, and utilities in EShop.Common.

## Scope
- [ ] Test `ValidationBehavior<TRequest, TResponse>`
  - [ ] Valid request passes through
  - [ ] Invalid request throws ValidationException
  - [ ] Multiple validation errors aggregated
- [ ] Test `LoggingBehavior<TRequest, TResponse>`
  - [ ] Request/response logging (verify via mock ILogger)
  - [ ] Elapsed time measurement
- [ ] Test `ExceptionHandlingMiddleware`
  - [ ] ValidationException → 400 Bad Request
  - [ ] NotFoundException → 404 Not Found
  - [ ] DomainException → 400 Bad Request
  - [ ] Unhandled exception → 500 Internal Server Error
- [ ] Test `CorrelationIdMiddleware`
  - [ ] Generates new CorrelationId if missing
  - [ ] Preserves existing CorrelationId from header
  - [ ] Sets CorrelationId in response header

## Dependencies
- Depends on: task-01
- Blocks: none

## Acceptance Criteria
- [ ] All MediatR behaviors have tests
- [ ] Exception middleware maps all exception types correctly
- [ ] CorrelationId middleware tests both generation and propagation

## Notes
- Use Moq to mock ILogger, RequestDelegate
- For middleware tests, create minimal HttpContext
- Consider testing pipeline order if relevant
