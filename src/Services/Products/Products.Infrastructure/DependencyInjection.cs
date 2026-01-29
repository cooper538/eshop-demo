using EShop.Common.Application.Data;
using EShop.Common.Infrastructure.Extensions;
using EShop.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Products.Application.Data;
using Products.Infrastructure.BackgroundJobs;
using Products.Infrastructure.Data;

namespace Products.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<ProductDbContext>(
            ResourceNames.Databases.Product,
            configureDbContextOptions: options =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    options.UseSeeding((context, _) => ProductDbContextSeed.Seed(context));
                    options.UseAsyncSeeding(
                        (context, _, ct) => ProductDbContextSeed.SeedAsync(context, ct)
                    );
                }
            }
        );

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
