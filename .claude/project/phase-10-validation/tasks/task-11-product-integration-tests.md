# Task 11: Product Integration Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-11 |
| Status | ⚪ pending |
| Dependencies | task-10, task-05 |

## Objective
Integration tests for Product Service API with real PostgreSQL database.

## Scope
- [ ] Create `tests/Product.IntegrationTests/` project
- [ ] Create `ProductApiFactory` (WebApplicationFactory)
  - [ ] Replace DbContext with Testcontainer PostgreSQL
  - [ ] Replace MassTransit with InMemory test harness
  - [ ] Apply migrations on startup
- [ ] Test Product API endpoints
  - [ ] POST /products - create product
  - [ ] GET /products/{id} - get product
  - [ ] GET /products - list products (with pagination)
  - [ ] PUT /products/{id} - update product
  - [ ] Validation error responses (400)
  - [ ] Not found responses (404)
- [ ] Test gRPC endpoints
  - [ ] ReserveStock - successful reservation
  - [ ] ReserveStock - insufficient stock
  - [ ] ReleaseStock
- [ ] Test database persistence
  - [ ] Product persisted correctly
  - [ ] Stock changes persisted
  - [ ] Concurrent stock operations

## Dependencies
- Depends on: task-10, task-05
- Blocks: task-14

## Acceptance Criteria
- [ ] All REST endpoints have integration tests
- [ ] gRPC endpoints tested
- [ ] Database state verified after operations
- [ ] Tests run in isolation (Respawn cleanup)

## Notes
- Use Respawn to reset database between tests
- For gRPC tests, use Grpc.Net.Client to call service
- Consider testing EF Core query behavior (pagination, filtering)
