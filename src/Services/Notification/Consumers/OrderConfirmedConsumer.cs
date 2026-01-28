using EShop.Contracts.Events.Order;
using EShop.NotificationService.Data;
using EShop.NotificationService.Services;
using EShop.SharedKernel.Services;
using MassTransit;

namespace EShop.NotificationService.Consumers;

public sealed class OrderConfirmedConsumer(
    NotificationDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    IEmailService emailService,
    ILogger<OrderConfirmedConsumer> logger
) : IdempotentConsumer<OrderConfirmedEvent>(dbContext, dateTimeProvider, logger)
{
    protected override async Task ProcessMessage(ConsumeContext<OrderConfirmedEvent> context)
    {
        var message = context.Message;

        var email = new EmailMessage(
            To: message.CustomerEmail,
            Subject: $"Order #{message.OrderId} Confirmed",
            HtmlBody: $"""
            <h1>Order Confirmed</h1>
            <p>Thank you for your order!</p>
            <p><strong>Order ID:</strong> {message.OrderId}</p>
            <p><strong>Total Amount:</strong> {message.TotalAmount:C}</p>
            <p>We will notify you when your order ships.</p>
            """
        );

        await emailService.SendAsync(email, context.CancellationToken);
    }
}
