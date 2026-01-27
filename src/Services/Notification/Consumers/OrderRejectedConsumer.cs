using EShop.Contracts.Events.Order;
using EShop.NotificationService.Data;
using EShop.NotificationService.Services;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EShop.NotificationService.Consumers;

public sealed class OrderRejectedConsumer(
    NotificationDbContext dbContext,
    IEmailService emailService,
    ILogger<OrderRejectedConsumer> logger
) : IdempotentConsumer<OrderRejectedEvent>(dbContext, logger)
{
    protected override async Task ProcessMessage(ConsumeContext<OrderRejectedEvent> context)
    {
        var message = context.Message;

        var email = new EmailMessage(
            To: message.CustomerEmail,
            Subject: $"Order #{message.OrderId} Rejected",
            HtmlBody: $"""
            <h1>Order Rejected</h1>
            <p>We're sorry, but your order could not be processed.</p>
            <p><strong>Order ID:</strong> {message.OrderId}</p>
            <p><strong>Reason:</strong> {message.Reason}</p>
            <p>Please contact support if you have any questions.</p>
            """
        );

        await emailService.SendAsync(email, context.CancellationToken);
    }
}
