# Phase 10: Testing & Validation

## Metadata
| Key | Value |
|-----|-------|
| Status | :white_circle: pending |

## Objective
Comprehensive testing across all layers and end-to-end validation

## Scope

### Unit Tests
- [ ] SharedKernel tests (Entity, ValueObject, Guard)
- [ ] EShop.Common tests (behaviors, middleware)
- [ ] Product Service domain tests
- [ ] Product Service stock operations tests
- [ ] Order Service domain tests
- [ ] Order Service lifecycle tests
- [ ] Notification consumers tests

### Integration Tests
- [ ] Product Service with mocked dependencies
- [ ] Order-Product integration with mocked Product Service
- [ ] MassTransit Test Harness tests

### Functional/E2E Tests
- [ ] WebApplicationFactory + Testcontainers setup
- [ ] Respawn for database cleanup
- [ ] Add WireMock for external API mocking (if needed)
- [ ] Complete order flow (Gateway → Order → Product → Notification)
- [ ] CorrelationId propagation end-to-end

### Documentation
- [ ] Document project startup

## Related Specs
- → [functional-testing.md](../high-level-specs/functional-testing.md)
- → [unit-testing.md](../high-level-specs/unit-testing.md)

---
## Notes
(Updated during implementation)
