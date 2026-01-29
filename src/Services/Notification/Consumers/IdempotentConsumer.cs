using EShop.NotificationService.Data;
using EShop.NotificationService.Data.Entities;
using EShop.SharedKernel.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EShop.NotificationService.Consumers;

// Inbox Pattern - ensures exactly-once processing per consumer
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

    protected virtual string ConsumerTypeName => GetType().Name;

    public async Task Consume(ConsumeContext<TMessage> context)
    {
        var messageId = context.MessageId ?? Guid.NewGuid();

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

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            context.CancellationToken
        );

        try
        {
            await ProcessMessage(context);

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

            _logger.LogInformation(
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

    protected abstract Task ProcessMessage(ConsumeContext<TMessage> context);
}
