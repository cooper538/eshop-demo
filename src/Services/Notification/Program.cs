using EShop.Common.Correlation.MassTransit;
using EShop.Common.Extensions;
using EShop.NotificationService.Configuration;
using EShop.NotificationService.Consumers;
using EShop.NotificationService.Data;
using EShop.NotificationService.Services;
using EShop.ServiceDefaults;
using MassTransit;
using NetEscapades.Configuration.Yaml;

var builder = Host.CreateApplicationBuilder(args);
var env = builder.Environment.EnvironmentName;

// YAML Configuration
builder
    .Configuration.AddYamlFile("notification.settings.yaml", optional: false, reloadOnChange: true)
    .AddYamlFile($"notification.settings.{env}.yaml", optional: true, reloadOnChange: true);

// Bind and validate settings (fail-fast on invalid config)
builder
    .Services.AddOptions<NotificationSettings>()
    .BindConfiguration(NotificationSettings.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Aspire ServiceDefaults (OpenTelemetry, Health Checks, Service Discovery)
builder.AddServiceDefaults();

// Aspire-managed PostgreSQL (connection string injected automatically)
builder.AddNpgsqlDbContext<NotificationDbContext>(ResourceNames.Databases.Notification);

// Email service (fake for development)
builder.Services.AddSingleton<IEmailService, FakeEmailService>();

// DateTime provider
builder.Services.AddDateTimeProvider();

// MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Order event consumers
    x.AddConsumer<OrderConfirmedConsumer>();
    x.AddConsumer<OrderRejectedConsumer>();
    x.AddConsumer<OrderCancelledConsumer>();

    // Stock event consumers
    x.AddConsumer<StockLowConsumer>();

    x.UsingRabbitMq(
        (context, cfg) =>
        {
            // Connection string injected via Aspire (ConnectionStrings__messaging)
            var connectionString = builder.Configuration.GetConnectionString(
                ResourceNames.Messaging
            );
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

            // CorrelationId propagation via message headers
            cfg.UseCorrelationIdFilters(context);

            cfg.ConfigureEndpoints(context);
        }
    );
});

var host = builder.Build();
host.Run();
