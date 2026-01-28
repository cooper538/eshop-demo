using EShop.Contracts.Events.Product;
using EShop.NotificationService.Configuration;
using EShop.NotificationService.Data;
using EShop.NotificationService.Services;
using EShop.SharedKernel.Services;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EShop.NotificationService.Consumers;

public sealed class StockLowConsumer(
    NotificationDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    IEmailService emailService,
    IOptions<NotificationSettings> options,
    ILogger<StockLowConsumer> logger
) : IdempotentConsumer<StockLowEvent>(dbContext, dateTimeProvider, logger)
{
    private readonly NotificationSettings _settings = options.Value;

    protected override async Task ProcessMessage(ConsumeContext<StockLowEvent> context)
    {
        var message = context.Message;

        var email = new EmailMessage(
            To: _settings.Email.AdminEmail,
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
