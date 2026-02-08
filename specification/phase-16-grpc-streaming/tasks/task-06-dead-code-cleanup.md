# Task 06: Dead Code Cleanup

## Metadata
| Key | Value |
|-----|-------|
| ID | task-06 |
| Status | ⬜ pending |
| Dependencies | task-01, task-02, task-03, task-04, task-05 |

## Summary
Delete all files made obsolete by the streaming conversion -- batch query/handler/result, DTO, and response mapper.

## Scope
- [ ] Delete `GetProductsBatchQuery.cs` from `Products.Application/Queries/GetProductsBatch/`
- [ ] Delete `GetProductsBatchQueryHandler.cs` from `Products.Application/Queries/GetProductsBatch/`
- [ ] Delete `GetProductsBatchResult.cs` from `Products.Application/Queries/GetProductsBatch/`
- [ ] Delete `GetProductsResponseMapper.cs` from `EShop.ServiceClients/Clients/Product/Mappers/`
- [ ] Delete `GetProductsResult.cs` from `EShop.Contracts/ServiceClients/Product/`
- [ ] Verify solution builds with no dangling references

## Related Specs
- → [product-service-interface.md](../../high-level-specs/product-service-interface.md)

---
## Notes
(Updated during implementation)
