using System.Data.Common;
using Npgsql;
using Respawn;

namespace EShop.Common.IntegrationTests.Fixtures;

#pragma warning disable CA1001 // IAsyncLifetime handles disposal via DisposeAsync
public sealed class DatabaseFixture : IAsyncLifetime
{
    private Respawner? _respawner;
    private DbConnection? _connection;
    private string? _connectionString;

    public void SetConnectionString(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InitializeAsync()
    {
        if (_connectionString is null)
        {
            throw new InvalidOperationException(
                "Connection string must be set before initializing DatabaseFixture. Call SetConnectionString() first."
            );
        }

        _connection = new NpgsqlConnection(_connectionString);
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(
            _connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"],
                TablesToIgnore = ["__EFMigrationsHistory"],
            }
        );
    }

    public async Task ResetAsync()
    {
        if (_respawner is null || _connection is null)
        {
            throw new InvalidOperationException(
                "DatabaseFixture must be initialized before calling ResetAsync()."
            );
        }

        await _respawner.ResetAsync(_connection);
    }

    public async Task DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
    }
}
