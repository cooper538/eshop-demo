# Task 2: YARP Configuration

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Summary
Configure YARP reverse proxy routes for Product and Order services.

## Scope
- [x] Configure YARP routes in `gateway.settings.yaml`
- [x] Set up routes for Product API (`/api/products/**` → product-cluster)
- [x] Set up routes for Order API (`/api/orders/**` → order-cluster)
- [x] Configure cluster endpoints using Aspire service discovery
- [x] Add health checks for downstream services (products-api, order-api)
- [x] Add output caching for GET requests (ProductsCache, ProductDetailCache, SwaggerCache)

## Related Specs
- → [internal-api-communication.md](../../high-level-specs/internal-api-communication.md)

---
## Notes
(Updated during implementation)
