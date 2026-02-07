using Microsoft.EntityFrameworkCore;

namespace EShop.DatabaseMigration;

public static class MigrationExtensions
{
    public static IHostApplicationBuilder AddMigratableDatabase<TContext>(
        this IHostApplicationBuilder builder,
        string connectionName
    )
        where TContext : DbContext
    {
        builder.AddNpgsqlDbContext<TContext>(connectionName);
        builder.Services.AddHostedService<DbInitializer<TContext>>();
        return builder;
    }
}
