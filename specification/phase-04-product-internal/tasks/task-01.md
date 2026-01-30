# Task 01: Update product.proto with gRPC Service Definition

## Metadata
| Key | Value |
|-----|-------|
| ID | task-01 |
| Status | ✅ completed |
| Dependencies | - |

## Summary
Define complete gRPC service for Product internal API with batch queries and stock operations.

## Scope
- [x] Create ProductService with GetProducts, ReserveStock, ReleaseStock RPCs
- [x] Define GetProductsRequest with repeated product_ids (string format)
- [x] Define GetProductsResponse with repeated ProductInfo
- [x] Define ProductInfo message (product_id, name, description, price as string, stock_quantity)
- [x] Define ReserveStockRequest with order_id and repeated OrderItem
- [x] Define OrderItem message (product_id, quantity)
- [x] Define ReserveStockResponse with success and optional failure_reason
- [x] Define ReleaseStockRequest with order_id
- [x] Define ReleaseStockResponse with success and optional failure_reason

## Implementation Details

**File**: `src/Common/EShop.Grpc/Protos/product.proto`

**Namespace**: `EShop.Grpc.Product`

**RPC Methods**:
| Method | Behavior |
|--------|----------|
| GetProducts | ATOMIC: fails with NOT_FOUND if any product missing |
| ReserveStock | All-or-nothing: fails if any item has insufficient stock |
| ReleaseStock | Idempotent: succeeds even if already released |

**Design Decision**:
- Price serialized as string (preserves decimal precision)
- GUID IDs serialized as string (standard gRPC practice)
- ATOMIC behavior per [Google AIP-231](https://google.aip.dev/231)

## Related Specs
- [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 3.1: gRPC Service Definition)
- [grpc-communication.md](../../high-level-specs/grpc-communication.md) (Section 2: Project Setup)

---
## Notes
(Updated during implementation)
