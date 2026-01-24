# Task 05: GetProductsBatch Query

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | :white_circle: pending |
| Dependencies | - |

## Summary
Implement batch product query for internal API - returns product info for multiple product IDs.

## Scope
- [ ] Create GetProductsBatchQuery record with list of product IDs
- [ ] Create GetProductsBatchResult record with list of ProductInfoDto
- [ ] Create ProductInfoDto (ProductId, Name, Description, Price, StockQuantity)
- [ ] Implement GetProductsBatchQueryHandler
- [ ] Return ProductInfo only for existing products
- [ ] Query handler returns only found products (caller validates completeness)

## Note
**ATOMIC validation happens in gRPC/HTTP layer, not in query handler.**
Query handler simply returns what exists. The gRPC service or controller
compares requested vs returned IDs and throws NOT_FOUND if any missing.

## Related Specs
- [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 3.1: GetProducts, Section 3.5: ServiceClients Models)

---
## Notes
(Updated during implementation)
