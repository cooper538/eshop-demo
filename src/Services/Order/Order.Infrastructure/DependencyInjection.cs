using EShop.Common.Application.Data;
using EShop.Common.Infrastructure.Data;
using EShop.Common.Infrastructure.Data.Interceptors;
using EShop.Common.Infrastructure.Extensions;
using EShop.Order.Application.Consumers;
using EShop.Order.Application.Data;
using EShop.Order.Infrastructure.BackgroundJobs;
using EShop.Order.Infrastructure.Data;
using EShop.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EShop.Order.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<DomainEventDispatchInterceptor>();

        // Environment-aware database configuration
        if (builder.Environment.IsProduction())
        {
            var connectionString = builder.Configuration.GetConnectionString(
                ResourceNames.Databases.Order
            );

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = PostgresConnectionStringBuilder.EnsureSslMode(connectionString);

                builder.Services.AddDbContext<OrderDbContext>(
                    (sp, options) =>
                    {
                        options.UseNpgsql(
                            connectionString,
                            npgsqlOptions =>
                            {
                                npgsqlOptions.EnableRetryOnFailure(
                                    maxRetryCount: 3,
                                    maxRetryDelay: TimeSpan.FromSeconds(10),
                                    errorCodesToAdd: null
                                );
                            }
                        );
                        options.AddInterceptors(
                            sp.GetRequiredService<DomainEventDispatchInterceptor>()
                        );
                    }
                );
            }
        }
        else
        {
            builder.Services.AddDbContext<OrderDbContext>(
                (sp, options) =>
                {
                    var connectionString = builder.Configuration.GetConnectionString(
                        ResourceNames.Databases.Order
                    );
                    options.UseNpgsql(connectionString);
                    options.AddInterceptors(
                        sp.GetRequiredService<DomainEventDispatchInterceptor>()
                    );
                }
            );

            // Add Aspire health checks, retries, and telemetry
            builder.EnrichNpgsqlDbContext<OrderDbContext>();
        }

        builder.Services.AddScoped<IOrderDbContext>(sp => sp.GetRequiredService<OrderDbContext>());

        builder.Services.AddScoped<IChangeTrackerAccessor>(sp =>
            sp.GetRequiredService<OrderDbContext>()
        );

        builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<OrderDbContext>());

        builder.Services.AddMessaging<OrderDbContext>(
            builder.Configuration,
            "order",
            configureConsumers: x =>
            {
                x.AddConsumer<ProductChangedConsumer>();
            }
        );

        builder.Services.AddFluentValidation(
            typeof(Order.Application.DependencyInjection).Assembly
        );

        builder.Services.AddHostedService<ProductSnapshotSyncJob>();

        return builder;
    }
}
