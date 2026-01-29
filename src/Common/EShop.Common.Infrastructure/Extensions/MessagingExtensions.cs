using EShop.Common.Infrastructure.Correlation.MassTransit;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Common.Infrastructure.Extensions;

public static class MessagingExtensions
{
    private const string DefaultMessagingConnectionName = "messaging";

    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        string endpointPrefix,
        Action<IBusRegistrationConfigurator>? configureConsumers = null,
        string connectionName = DefaultMessagingConnectionName
    )
    {
        services.AddMassTransit(x =>
        {
            configureConsumers?.Invoke(x);

            x.UsingRabbitMq(
                (context, cfg) =>
                {
                    var connectionString = configuration.GetConnectionString(connectionName);
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
                    cfg.ConfigureEndpoints(
                        context,
                        new KebabCaseEndpointNameFormatter(endpointPrefix, false)
                    );
                }
            );
        });

        return services;
    }

    public static IServiceCollection AddMessaging<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string endpointPrefix,
        Action<IBusRegistrationConfigurator>? configureConsumers = null,
        string connectionName = DefaultMessagingConnectionName
    )
        where TDbContext : DbContext
    {
        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<TDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

            configureConsumers?.Invoke(x);

            x.UsingRabbitMq(
                (context, cfg) =>
                {
                    var connectionString = configuration.GetConnectionString(connectionName);
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
                    cfg.ConfigureEndpoints(
                        context,
                        new KebabCaseEndpointNameFormatter(endpointPrefix, false)
                    );
                }
            );
        });

        return services;
    }
}
