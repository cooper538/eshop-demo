using EShop.Common.Application.Data;
using EShop.Common.Infrastructure.Data.Interceptors;
using EShop.Common.Infrastructure.Extensions;
using EShop.Order.Application.Data;
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

        builder.Services.AddDbContext<OrderDbContext>(
            (sp, options) =>
            {
                var connectionString = builder.Configuration.GetConnectionString(
                    ResourceNames.Databases.Order
                );
                options.UseNpgsql(connectionString);
                options.AddInterceptors(sp.GetRequiredService<DomainEventDispatchInterceptor>());
            }
        );

        // Add Aspire health checks, retries, and telemetry
        builder.EnrichNpgsqlDbContext<OrderDbContext>();

        builder.Services.AddScoped<IOrderDbContext>(sp => sp.GetRequiredService<OrderDbContext>());

        builder.Services.AddScoped<IChangeTrackerAccessor>(sp =>
            sp.GetRequiredService<OrderDbContext>()
        );

        builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<OrderDbContext>());

        builder.Services.AddMessaging<OrderDbContext>(builder.Configuration, "order");

        builder.Services.AddFluentValidation(
            typeof(Order.Application.DependencyInjection).Assembly
        );

        return builder;
    }
}
