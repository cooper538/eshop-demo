using EShop.Products.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EShop.DatabaseMigration;

public static class MigrationExtensions
{
    public static IHostApplicationBuilder AddMigratableDatabase<TContext>(
        this IHostApplicationBuilder builder,
        string connectionName,
        Action<DbContextOptionsBuilder>? configureDbContextOptions = null
    )
        where TContext : DbContext
    {
        builder.AddNpgsqlDbContext<TContext>(
            connectionName,
            configureDbContextOptions: configureDbContextOptions
        );
        builder.Services.AddHostedService<DbInitializer<TContext>>();
        return builder;
    }

    public static IHostApplicationBuilder AddProductSeeding(this IHostApplicationBuilder builder)
    {
        if (!builder.Environment.IsDevelopment())
        {
            return builder;
        }

        var settings =
            builder.Configuration.GetSection("Seeding").Get<SeedingSettings>()
            ?? new SeedingSettings();

        builder.Services.ConfigureDbContext<ProductDbContext>(options =>
            options.UseAsyncSeeding(
                async (context, _, ct) =>
                {
                    await ProductDbContextSeed.SeedAsync(
                        (ProductDbContext)context,
                        settings.ProductCount,
                        settings.BatchSize,
                        ct
                    );
                }
            )
        );

        return builder;
    }
}
