using EShop.Common.Data;
using EShop.ServiceDefaults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Order.Application.Data;
using Order.Infrastructure.Data;

namespace Order.Infrastructure;

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

        return builder;
    }
}
