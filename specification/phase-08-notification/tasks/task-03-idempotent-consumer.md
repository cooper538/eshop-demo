# Task 3: Idempotent Consumer Base

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ✅ completed |
| Dependencies | task-02 |

## Summary
Create IdempotentConsumer<T> base class that implements inbox pattern for all consumers.

## Scope
- [x] Create `IdempotentConsumer<TMessage>` abstract base class implementing `IConsumer<TMessage>`
- [x] Implement message deduplication check using `NotificationDbContext`
- [x] Use EF Core execution strategy for resilient database operations
- [x] Wrap processing in database transaction
- [x] Record processed message after successful handling
- [x] Add logging for duplicate message detection
- [x] Add logging for successful message processing
- [x] Define abstract `ProcessMessage(ConsumeContext<TMessage>)` method for derived classes
- [x] Use `IDateTimeProvider` for testable timestamps

## Implementation
```csharp
public abstract class IdempotentConsumer<TMessage> : IConsumer<TMessage>
    where TMessage : class
{
    protected virtual string ConsumerTypeName => GetType().Name;

    public async Task Consume(ConsumeContext<TMessage> context)
    {
        var messageId = context.MessageId ?? Guid.NewGuid();

        // Check for duplicate
        var alreadyProcessed = await _dbContext.ProcessedMessages.AnyAsync(
            pm => pm.MessageId == messageId && pm.ConsumerType == ConsumerTypeName);

        if (alreadyProcessed) { /* Log and skip */ return; }

        // Execute with retry strategy and transaction
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await ProcessMessage(context);
                _dbContext.ProcessedMessages.Add(new ProcessedMessage { ... });
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    protected abstract Task ProcessMessage(ConsumeContext<TMessage> context);
}
```

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section: 6.3. Idempotent Consumer Base Class)

---
## Notes
- Uses `CreateExecutionStrategy()` for PostgreSQL retry resilience
- Transaction ensures atomicity between message processing and inbox record
- `ConsumerTypeName` virtual property allows customization if needed
