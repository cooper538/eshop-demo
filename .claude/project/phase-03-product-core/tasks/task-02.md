# Task 02: Domain Model

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Summary
Implement Product entity with all properties and business logic methods.

## Scope
- [ ] Create Product entity inheriting from Entity (EShop.SharedKernel)
- [ ] Add properties: Name, Description, Price, StockQuantity, LowStockThreshold, Category, CreatedAt, UpdatedAt, Version
- [ ] Implement ReserveStock(quantity) method returning bool
- [ ] Implement ReleaseStock(quantity) method
- [ ] Implement IsLowStock computed property
- [ ] Add factory method Product.Create(...)
- [ ] Create InsufficientStockException in Domain/Exceptions

## Related Specs
- → [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 5: Domain Model)

---
## Notes
(Updated during implementation)
