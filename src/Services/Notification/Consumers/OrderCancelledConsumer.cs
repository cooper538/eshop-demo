using EShop.Contracts.Events.Order;
using EShop.NotificationService.Data;
using EShop.NotificationService.Services;
using EShop.SharedKernel.Services;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EShop.NotificationService.Consumers;

public sealed class OrderCancelledConsumer(
    NotificationDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    IEmailService emailService,
    ILogger<OrderCancelledConsumer> logger
) : IdempotentConsumer<OrderCancelledEvent>(dbContext, dateTimeProvider, logger)
{
    protected override async Task ProcessMessage(ConsumeContext<OrderCancelledEvent> context)
    {
        var message = context.Message;

        var email = new EmailMessage(
            To: message.CustomerEmail,
            Subject: $"Order #{message.OrderId} Cancelled",
            HtmlBody: $"""
            <h1>Order Cancelled</h1>
            <p>Your order has been cancelled.</p>
            <p><strong>Order ID:</strong> {message.OrderId}</p>
            <p><strong>Reason:</strong> {message.Reason}</p>
            <p>If you did not request this cancellation, please contact support.</p>
            """
        );

        await emailService.SendAsync(email, context.CancellationToken);
    }
}
