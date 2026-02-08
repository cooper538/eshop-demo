using EShop.Common.Application.Data;
using EShop.Common.Infrastructure.Data;
using EShop.Common.Infrastructure.Data.Interceptors;
using EShop.Common.Infrastructure.Extensions;
using EShop.Products.Application.Data;
using EShop.Products.Infrastructure.BackgroundJobs;
using EShop.Products.Infrastructure.Data;
using EShop.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EShop.Products.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<DomainEventDispatchInterceptor>();

        // Environment-aware database configuration
        if (builder.Environment.IsProduction())
        {
            var connectionString = builder.Configuration.GetConnectionString(
                ResourceNames.Databases.Product
            );

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = PostgresConnectionStringBuilder.EnsureSslMode(connectionString);

                builder.Services.AddDbContext<ProductDbContext>(
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
            builder.Services.AddDbContext<ProductDbContext>(
                (sp, options) =>
                {
                    var connectionString = builder.Configuration.GetConnectionString(
                        ResourceNames.Databases.Product
                    );
                    options.UseNpgsql(connectionString);
                    options.AddInterceptors(
                        sp.GetRequiredService<DomainEventDispatchInterceptor>()
                    );
                }
            );

            builder.EnrichNpgsqlDbContext<ProductDbContext>();
        }

        builder.Services.AddScoped<IProductDbContext>(sp =>
            sp.GetRequiredService<ProductDbContext>()
        );

        builder.Services.AddScoped<IChangeTrackerAccessor>(sp =>
            sp.GetRequiredService<ProductDbContext>()
        );

        builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ProductDbContext>());

        builder.Services.AddMessaging<ProductDbContext>(builder.Configuration, "products");

        builder.Services.AddHostedService<StockReservationExpirationJob>();

        builder.Services.AddFluentValidation(
            typeof(Products.Application.DependencyInjection).Assembly
        );

        return builder;
    }
}
