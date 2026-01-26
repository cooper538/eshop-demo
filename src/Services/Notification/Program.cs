using EShop.NotificationService.Consumers;
using EShop.NotificationService.Data;
using EShop.NotificationService.Services;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);

// Aspire ServiceDefaults (OpenTelemetry, Health Checks, Service Discovery)
builder.AddServiceDefaults();

// Aspire-managed PostgreSQL (connection string injected automatically)
builder.AddNpgsqlDbContext<NotificationDbContext>("notificationdb");

// Email service (fake for development)
builder.Services.AddSingleton<IEmailService, FakeEmailService>();

// MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Order event consumers
    x.AddConsumer<OrderConfirmedConsumer>();
    x.AddConsumer<OrderRejectedConsumer>();
    x.AddConsumer<OrderCancelledConsumer>();

    // Stock event consumers (task-06)
    // x.AddConsumer<StockLowConsumer>();

    x.UsingRabbitMq(
        (context, cfg) =>
        {
            // Connection string injected via Aspire (ConnectionStrings__messaging)
            var connectionString = builder.Configuration.GetConnectionString("messaging");
            if (!string.IsNullOrEmpty(connectionString))
            {
                cfg.Host(new Uri(connectionString));
            }

            // Retry policy for transient failures
            cfg.UseMessageRetry(r =>
                r.Intervals(
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(15)
                )
            );

            cfg.ConfigureEndpoints(context);
        }
    );
});

var host = builder.Build();
host.Run();
