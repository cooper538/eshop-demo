using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EShop.Common.Infrastructure.Data;

/// <summary>
/// Extension methods for configuring PostgreSQL database connections
/// in Azure environments with SSL support.
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Adds a PostgreSQL DbContext configured for Azure deployment with SSL mode enabled.
    /// Use this method for Azure environments. For local development with Aspire,
    /// use the standard AddNpgsqlDbContext from Aspire package.
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext type to register</typeparam>
    /// <param name="builder">The host application builder</param>
    /// <param name="connectionName">The name of the connection string in configuration</param>
    /// <returns>The host application builder for chaining</returns>
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
            throw new InvalidOperationException(
                $"Connection string '{connectionName}' not found in configuration."
            );
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

    /// <summary>
    /// Adds a PostgreSQL DbContext with pooling configured for Azure deployment.
    /// Recommended for high-throughput scenarios.
    /// </summary>
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
            throw new InvalidOperationException(
                $"Connection string '{connectionName}' not found in configuration."
            );
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
