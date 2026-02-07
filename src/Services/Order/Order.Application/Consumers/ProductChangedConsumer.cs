using EShop.Common.Application.Data;
using EShop.Contracts.IntegrationEvents.Product;
using EShop.Order.Application.Data;
using EShop.Order.Domain.ReadModels;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EShop.Order.Application.Consumers;

public sealed class ProductChangedConsumer(
    IOrderDbContext dbContext,
    IUnitOfWork unitOfWork,
    ILogger<ProductChangedConsumer> logger
) : IConsumer<ProductChangedEvent>
{
    public async Task Consume(ConsumeContext<ProductChangedEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Received ProductChangedEvent for Product {ProductId} ({ProductName})",
            message.ProductId,
            message.Name
        );

        var existing = await dbContext.ProductSnapshots.FindAsync(
            [message.ProductId],
            context.CancellationToken
        );

        if (existing is not null)
        {
            if (message.Timestamp <= existing.LastUpdated)
            {
                logger.LogDebug(
                    "Skipping stale ProductChangedEvent for Product {ProductId}",
                    message.ProductId
                );
                return;
            }

            existing.Update(message.Name, message.Price, message.Timestamp);
        }
        else
        {
            dbContext.ProductSnapshots.Add(
                ProductSnapshot.Create(
                    message.ProductId,
                    message.Name,
                    message.Price,
                    message.Timestamp
                )
            );
        }

        await unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
