using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EShop.Common.Infrastructure.Data;

public static class DatabaseExtensions
{
    public static IHostApplicationBuilder AddNpgsqlDbContextAzure<TDbContext>(
        this IHostApplicationBuilder builder,
        string connectionName
    )
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(builder);

        var connectionString = builder.Configuration.GetConnectionString(connectionName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Design-time build (OpenAPI generation) - skip DB registration gracefully
            return builder;
        }

        connectionString = PostgresConnectionStringBuilder.EnsureSslMode(connectionString);

        builder.Services.AddDbContext<TDbContext>(options =>
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
        });

        return builder;
    }

    public static IHostApplicationBuilder AddNpgsqlDbContextPoolAzure<TDbContext>(
        this IHostApplicationBuilder builder,
        string connectionName,
        int poolSize = 128
    )
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(builder);

        var connectionString = builder.Configuration.GetConnectionString(connectionName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return builder;
        }

        connectionString = PostgresConnectionStringBuilder.EnsureSslMode(connectionString);

        builder.Services.AddDbContextPool<TDbContext>(
            options =>
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
            },
            poolSize
        );

        return builder;
    }
}
