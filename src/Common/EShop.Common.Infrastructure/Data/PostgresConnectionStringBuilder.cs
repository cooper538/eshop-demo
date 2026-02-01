using Npgsql;

namespace EShop.Common.Infrastructure.Data;

/// <summary>
/// Utility class for building and modifying PostgreSQL connection strings
/// with Azure-specific requirements (SSL mode, etc.).
/// </summary>
public static class PostgresConnectionStringBuilder
{
    /// <summary>
    /// Ensures the connection string has SSL mode set to Require for Azure PostgreSQL.
    /// </summary>
    /// <param name="connectionString">Original connection string</param>
    /// <param name="requireSsl">Whether to require SSL (default: true)</param>
    /// <returns>Connection string with SSL mode configured</returns>
    public static string EnsureSslMode(string connectionString, bool requireSsl = true)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        if (requireSsl)
        {
            builder.SslMode = SslMode.Require;
        }

        return builder.ConnectionString;
    }

    /// <summary>
    /// Builds an Azure PostgreSQL connection string from individual parameters.
    /// </summary>
    public static string BuildAzureConnectionString(
        string host,
        string database,
        string username,
        string password,
        int port = 5432
    )
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Database = database,
            Username = username,
            Password = password,
            SslMode = SslMode.Require,
        };

        return builder.ConnectionString;
    }

    /// <summary>
    /// Checks if a connection string is configured for SSL.
    /// </summary>
    public static bool HasSslEnabled(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return false;
        }

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        return builder.SslMode != SslMode.Disable;
    }
}
