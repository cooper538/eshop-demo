# Task 01: PostgreSQL SSL Configuration

## Metadata
| Key | Value |
|-----|-------|
| ID | task-01 |
| Status | in_progress |
| Dependencies | - |

## Summary
Add SSL mode handling for Azure PostgreSQL Flexible Server connections with automatic configuration based on environment.

## Scope
- [ ] Create `PostgresConnectionStringBuilder` utility class
- [ ] Implement SSL mode detection and injection for Azure connections
- [ ] Add `SslMode=Require` for Azure PostgreSQL connections
- [ ] Handle both connection string and individual parameters
- [ ] Create extension method for DbContext configuration with SSL support
- [ ] Update existing `AddDatabase<T>()` extension to support Azure mode

## Implementation Notes

Azure PostgreSQL Flexible Server requires SSL by default. Connection strings need `SslMode=Require` parameter.

```csharp
// PostgresConnectionStringBuilder.cs
public static class PostgresConnectionStringBuilder
{
    public static string EnsureSslMode(string connectionString, bool requireSsl = true)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        if (requireSsl && builder.SslMode == SslMode.Disable)
        {
            builder.SslMode = SslMode.Require;
        }

        return builder.ConnectionString;
    }

    public static string BuildAzureConnectionString(
        string host,
        string database,
        string username,
        string password)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Database = database,
            Username = username,
            Password = password,
            SslMode = SslMode.Require,
            TrustServerCertificate = true // Azure uses Microsoft-signed certs
        };

        return builder.ConnectionString;
    }
}

// Extension method for environment-aware database configuration
public static IHostApplicationBuilder AddDatabaseAzure<TDbContext>(
    this IHostApplicationBuilder builder,
    string connectionName)
    where TDbContext : DbContext
{
    var connectionString = builder.Configuration
        .GetConnectionString(connectionName);

    connectionString = PostgresConnectionStringBuilder
        .EnsureSslMode(connectionString, requireSsl: true);

    builder.Services.AddDbContext<TDbContext>(options =>
        options.UseNpgsql(connectionString));

    return builder;
}
```

## Connection String Examples

| Environment | Connection String |
|-------------|-------------------|
| Local (Aspire) | `Host=localhost;Database=productdb;Username=postgres;Password=xxx` |
| Azure | `Host=eshop-postgres.postgres.database.azure.com;Database=productdb;Username=eshop@eshop-postgres;Password=xxx;SslMode=Require` |

## Files to Create/Modify

| Action | File |
|--------|------|
| CREATE | `src/Common/EShop.Common.Infrastructure/Data/PostgresConnectionStringBuilder.cs` |
| MODIFY | `src/Common/EShop.Common.Infrastructure/Data/DatabaseExtensions.cs` |

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 3. Data Resources)

---
## Notes
- TrustServerCertificate is obsolete in Npgsql 9.x - removed from implementation
- Created `AddNpgsqlDbContextAzure<T>()` extension (not modifying Aspire's extension)
- Added `AddNpgsqlDbContextPoolAzure<T>()` for high-throughput scenarios
- Retry policy: 3 retries, max 10s delay
