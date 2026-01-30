namespace EShop.Common.IntegrationTests.Fixtures;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected PostgresContainerFixture Postgres { get; }
    protected RabbitMqContainerFixture RabbitMq { get; }
    protected DatabaseFixture Database { get; }

    protected IntegrationTestBase(
        PostgresContainerFixture postgres,
        RabbitMqContainerFixture rabbitMq
    )
    {
        Postgres = postgres;
        RabbitMq = rabbitMq;
        Database = new DatabaseFixture();
    }

    public virtual async Task InitializeAsync()
    {
        Database.SetConnectionString(Postgres.ConnectionString);
        await Database.InitializeAsync();
        await Database.ResetAsync();
    }

    public virtual async Task DisposeAsync()
    {
        await Database.DisposeAsync();
    }

    protected async Task ResetDatabaseAsync()
    {
        await Database.ResetAsync();
    }
}
