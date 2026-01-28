using EShop.Common.Data;
using EShop.ServiceDefaults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Products.Application.Data;
using Products.Infrastructure.Data;

namespace Products.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<ProductDbContext>(ResourceNames.Databases.Product);

        builder.Services.AddScoped<IProductDbContext>(sp =>
            sp.GetRequiredService<ProductDbContext>()
        );

        builder.Services.AddScoped<IChangeTrackerAccessor>(sp =>
            sp.GetRequiredService<ProductDbContext>()
        );

        return builder;
    }
}
