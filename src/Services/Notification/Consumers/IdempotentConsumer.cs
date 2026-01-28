using EShop.NotificationService.Data;
using EShop.NotificationService.Data.Entities;
using EShop.SharedKernel.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EShop.NotificationService.Consumers;

/// <summary>
/// Abstract base class for idempotent message consumers implementing the Inbox Pattern.
/// Ensures each message is processed exactly once per consumer type.
/// </summary>
/// <typeparam name="TMessage">The type of message to consume.</typeparam>
public abstract class IdempotentConsumer<TMessage> : IConsumer<TMessage>
    where TMessage : class
{
    private readonly NotificationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger _logger;

    protected IdempotentConsumer(
        NotificationDbContext dbContext,
        IDateTimeProvider dateTimeProvider,
        ILogger logger
    )
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    /// <summary>
    /// Gets the consumer type name used for deduplication.
    /// Defaults to the concrete class name.
    /// </summary>
    protected virtual string ConsumerTypeName => GetType().Name;

    public async Task Consume(ConsumeContext<TMessage> context)
    {
        var messageId = context.MessageId ?? Guid.NewGuid();

        // Check if message was already processed by this consumer
        var alreadyProcessed = await _dbContext.ProcessedMessages.AnyAsync(
            pm => pm.MessageId == messageId && pm.ConsumerType == ConsumerTypeName,
            context.CancellationToken
        );

        if (alreadyProcessed)
        {
            _logger.LogInformation(
                "Duplicate message detected. MessageId: {MessageId}, Consumer: {ConsumerType}. Skipping.",
                messageId,
                ConsumerTypeName
            );
            return;
        }

        // Process message within a transaction
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            context.CancellationToken
        );

        try
        {
            // Execute the actual message processing logic
            await ProcessMessage(context);

            // Record the message as processed
            _dbContext.ProcessedMessages.Add(
                new ProcessedMessage
                {
                    MessageId = messageId,
                    ConsumerType = ConsumerTypeName,
                    ProcessedAt = _dateTimeProvider.UtcNow,
                }
            );

            await _dbContext.SaveChangesAsync(context.CancellationToken);
            await transaction.CommitAsync(context.CancellationToken);

            _logger.LogDebug(
                "Message processed successfully. MessageId: {MessageId}, Consumer: {ConsumerType}",
                messageId,
                ConsumerTypeName
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(context.CancellationToken);

            _logger.LogError(
                ex,
                "Error processing message. MessageId: {MessageId}, Consumer: {ConsumerType}",
                messageId,
                ConsumerTypeName
            );

            throw;
        }
    }

    /// <summary>
    /// Implement this method in derived classes to handle the actual message processing logic.
    /// This method is called within a database transaction.
    /// </summary>
    /// <param name="context">The consume context containing the message.</param>
    protected abstract Task ProcessMessage(ConsumeContext<TMessage> context);
}
