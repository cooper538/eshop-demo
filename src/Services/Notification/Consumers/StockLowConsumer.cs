using EShop.Contracts.Events.Product;
using EShop.NotificationService.Data;
using EShop.NotificationService.Services;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EShop.NotificationService.Consumers;

public sealed class StockLowConsumer(
    NotificationDbContext dbContext,
    IEmailService emailService,
    ILogger<StockLowConsumer> logger
) : IdempotentConsumer<StockLowEvent>(dbContext, logger)
{
    // TODO: Move to configuration (appsettings.json or environment variable)
    private const string AdminEmail = "admin@eshop.local";

    protected override async Task ProcessMessage(ConsumeContext<StockLowEvent> context)
    {
        var message = context.Message;

        var email = new EmailMessage(
            To: AdminEmail,
            Subject: $"Low Stock Alert: {message.ProductName}",
            HtmlBody: $"""
            <h1>Low Stock Alert</h1>
            <p>The following product is running low on stock:</p>
            <p><strong>Product:</strong> {message.ProductName}</p>
            <p><strong>Product ID:</strong> {message.ProductId}</p>
            <p><strong>Current Quantity:</strong> {message.CurrentQuantity}</p>
            <p><strong>Threshold:</strong> {message.Threshold}</p>
            <p>Please restock as soon as possible.</p>
            """
        );

        await emailService.SendAsync(email, context.CancellationToken);
    }
}
