using EShop.Common.Application.Extensions;
using EShop.Common.Infrastructure.Correlation.MassTransit;
using EShop.Common.Infrastructure.Extensions;
using EShop.NotificationService.Configuration;
using EShop.NotificationService.Consumers;
using EShop.NotificationService.Data;
using EShop.NotificationService.Services;
using EShop.ServiceDefaults;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EShop.NotificationService;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddNotificationServices(
        this IHostApplicationBuilder builder
    )
    {
        builder
            .Services.AddOptions<NotificationSettings>()
            .BindConfiguration(NotificationSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.AddNpgsqlDbContext<NotificationDbContext>(ResourceNames.Databases.Notification);

        builder.Services.AddSingleton<IEmailService, FakeEmailService>();
        builder.Services.AddDateTimeProvider();

        builder.Services.AddNotificationMessaging(builder.Configuration);

        return builder;
    }

    private static IServiceCollection AddNotificationMessaging(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderConfirmedConsumer>();
            x.AddConsumer<OrderRejectedConsumer>();
            x.AddConsumer<OrderCancelledConsumer>();
            x.AddConsumer<StockLowConsumer>();

            x.UsingRabbitMq(
                (context, cfg) =>
                {
                    var connectionString = configuration.GetConnectionString(
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

        return services;
    }
}
