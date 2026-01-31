using EShop.Common.Application.Data;
using EShop.Common.Infrastructure.Extensions;
using EShop.Order.Application.Data;
using EShop.Order.Infrastructure.Data;
using EShop.ServiceDefaults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EShop.Order.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<OrderDbContext>(ResourceNames.Databases.Order);

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
