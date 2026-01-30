using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
#pragma warning disable IDE0130 // Namespace intentionally differs - extension methods for Microsoft.Extensions.Hosting
namespace Microsoft.Extensions.Hosting;

public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds a PostgreSQL health check using the specified connection string name.
    /// </summary>
    public static IHealthChecksBuilder AddPostgresHealthCheck(
        this IHealthChecksBuilder builder,
        string connectionStringName
    )
    {
        return builder.AddNpgSql(
            name: $"postgres-{connectionStringName}",
            connectionStringFactory: sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                return configuration.GetConnectionString(connectionStringName)
                    ?? throw new InvalidOperationException(
                        $"Connection string '{connectionStringName}' not found."
                    );
            },
            tags: ["ready"]
        );
    }

    /// <summary>
    /// Adds an HTTP health check for a downstream service.
    /// </summary>
    public static IHealthChecksBuilder AddServiceHealthCheck(
        this IHealthChecksBuilder builder,
        string serviceName,
        string healthEndpoint = "/health"
    )
    {
        return builder.AddUrlGroup(
            name: $"service-{serviceName}",
            uri: new Uri($"http://{serviceName}{healthEndpoint}"),
            configureClient: (sp, client) =>
            {
                client.Timeout = TimeSpan.FromSeconds(5);
            },
            tags: ["ready"]
        );
    }
}
