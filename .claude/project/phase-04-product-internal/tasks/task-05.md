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
- [ ] Create ProductInfoDto (ProductId, Name, Description, Price, StockQuantity, Exists, IsAvailable)
- [ ] Implement GetProductsBatchQueryHandler
- [ ] Return ProductInfo for each requested ID
- [ ] Handle non-existent products (Exists=false)
- [ ] Set IsAvailable based on StockQuantity > 0

## Related Specs
- [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 3.1: GetProducts, Section 3.5: ServiceClients Models)

---
## Notes
(Updated during implementation)
