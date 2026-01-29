using EShop.Contracts.IntegrationEvents.Order;
using MassTransit;

namespace EShop.AnalyticsService.Consumers;

public sealed class OrderConfirmedConsumer(ILogger<OrderConfirmedConsumer> logger)
    : IConsumer<OrderConfirmedEvent>
{
    public Task Consume(ConsumeContext<OrderConfirmedEvent> context)
    {
        logger.LogInformation(
            "Analytics: Received OrderConfirmedEvent for order {OrderId}",
            context.Message.OrderId
        );

        return Task.CompletedTask;
    }
}
