# Task 07: Stock Reservation Expiration Job

## Metadata
| Key | Value |
|-----|-------|
| ID | task-07 |
| Status | ✅ completed |
| Dependencies | task-03, task-04 |

## Summary
Background job that expires stale stock reservations and releases stock back to inventory.

## Scope
- [x] Create StockReservationExpirationJob extending BackgroundService
- [x] Implement ExecuteAsync with configurable check interval
- [x] Create ExpireReservationsCommand for batch processing
- [x] Implement ExpireReservationsCommandHandler
- [x] Query for Active reservations where ExpiresAt < UtcNow
- [x] Delegate expiration to Stock.ExpireStaleReservations()
- [x] Handle concurrency conflicts gracefully (retry on next run)
- [x] Add structured logging for expired reservations
- [x] Register as hosted service

## Implementation Details

**Files**:
- `Products.Infrastructure/BackgroundJobs/StockReservationExpirationJob.cs`
- `Products.Application/Commands/ExpireReservations/ExpireReservationsCommand.cs`
- `Products.Application/Commands/ExpireReservations/ExpireReservationsCommandHandler.cs`

**Architecture**:
```
BackgroundService -> MediatR -> ExpireReservationsCommandHandler -> Stock.ExpireStaleReservations()
```

**Configuration** (IStockReservationOptions):
| Option | Description |
|--------|-------------|
| DefaultDuration | Reservation TTL (default 15 min) |
| Expiration.CheckInterval | How often to check for expired (default 1 min) |
| Expiration.BatchSize | Max reservations per run |

**Error Handling**:
- ConflictException (concurrency) - logged as warning, retried next run
- OperationCanceledException - graceful shutdown
- Other exceptions - logged as error, retried next run

**Logging** (source-generated):
- LogJobStarted / LogJobStopped
- LogFoundExpiredReservations(count)
- LogReservationExpired(orderId, productId, quantity)
- LogProcessingCompleted(count)
- LogConcurrencyConflict / LogProcessingError

## Related Specs
- [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 6: Stock Reservation Expiration)

---
## Notes
(Updated during implementation)
