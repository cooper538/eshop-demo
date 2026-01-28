using EShop.Common.Correlation.MassTransit;
using EShop.Common.Extensions;
using EShop.NotificationService.Configuration;
using EShop.NotificationService.Consumers;
using EShop.NotificationService.Data;
using EShop.NotificationService.Services;
using EShop.ServiceDefaults;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);
var env = builder.Environment.EnvironmentName;

builder
    .Configuration.AddYamlFile("notification.settings.yaml", optional: false, reloadOnChange: true)
    .AddYamlFile($"notification.settings.{env}.yaml", optional: true, reloadOnChange: true);

builder
    .Services.AddOptions<NotificationSettings>()
    .BindConfiguration(NotificationSettings.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.AddServiceDefaults();
builder.AddSerilog();

builder.AddNpgsqlDbContext<NotificationDbContext>(ResourceNames.Databases.Notification);

builder.Services.AddSingleton<IEmailService, FakeEmailService>();

builder.Services.AddDateTimeProvider();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderConfirmedConsumer>();
    x.AddConsumer<OrderRejectedConsumer>();
    x.AddConsumer<OrderCancelledConsumer>();

    x.AddConsumer<StockLowConsumer>();

    x.UsingRabbitMq(
        (context, cfg) =>
        {
            var connectionString = builder.Configuration.GetConnectionString(
                ResourceNames.Messaging
            );
            if (!string.IsNullOrEmpty(connectionString))
            {
                cfg.Host(new Uri(connectionString));
            }

            cfg.UseMessageRetry(r =>
                r.Intervals(
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(15)
                )
            );

            cfg.UseCorrelationIdFilters(context);

            cfg.ConfigureEndpoints(context);
        }
    );
});

var host = builder.Build();
host.Run();
