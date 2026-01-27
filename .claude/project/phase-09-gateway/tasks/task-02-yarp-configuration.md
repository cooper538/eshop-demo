# Task 2: YARP Configuration

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ⚪ pending |
| Dependencies | task-01 |

## Summary
Configure YARP reverse proxy routes for Product and Order services.

## Scope
- [ ] Configure YARP routes in `appsettings.json`
- [ ] Set up route for Product API (`/api/products/**` → ProductService)
- [ ] Set up route for Order API (`/api/orders/**` → OrderService)
- [ ] Configure cluster endpoints using Aspire service discovery
- [ ] Add health check endpoint for gateway
- [ ] Test routing to both services

## Related Specs
- → [internal-api-communication.md](../../high-level-specs/internal-api-communication.md)

---
## Notes
(Updated during implementation)
