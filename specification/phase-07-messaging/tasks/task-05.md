# Task 05: MassTransit CorrelationId Filters

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | ✅ completed |
| Dependencies | - |

## Summary
Implement MassTransit publish/consume filters for automatic CorrelationId propagation via message headers.

## Scope
- [x] Create `CorrelationIdPublishFilter<T>` that adds CorrelationId to message headers
- [x] Create `CorrelationIdSendFilter<T>` for Send operations
- [x] Create `CorrelationIdConsumeFilter<T>` that extracts CorrelationId and sets ambient context
- [x] Create `MassTransitCorrelationExtensions` with `UseCorrelationIdFilters()` extension method
- [x] All services using MassTransit configured with `UseCorrelationIdFilters()`
- [x] Verify solution builds successfully

## Related Specs
- → [correlation-id-flow.md](../../high-level-specs/correlation-id-flow.md) (Section 6: Messaging Layer)

---
## Notes
- Location: `src/Common/EShop.Common.Infrastructure/Correlation/MassTransit/`
- `CorrelationIdPublishFilter<T>` - sets header on publish
- `CorrelationIdSendFilter<T>` - sets header on send
- `CorrelationIdConsumeFilter<T>` - extracts from header, creates `CorrelationContext.Scope`, adds to logging scope
- Header key: `X-Correlation-ID` (from `CorrelationIdConstants.MassTransitHeaderKey`)
- Filters auto-generate new CorrelationId if not present
