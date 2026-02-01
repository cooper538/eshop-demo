using EShop.Order.Infrastructure.Data;
using EShop.Order.IntegrationTests.Fixtures;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Order.IntegrationTests.Infrastructure;

[Collection(OrderIntegrationTestCollection.Name)]
#pragma warning disable CA1001 // IAsyncLifetime handles disposal via DisposeAsync
public abstract class OrderIntegrationTestBase : IAsyncLifetime
{
    private readonly OrderApiFactory _factory;
    private readonly HttpClient _client;

    protected OrderApiFactory Factory => _factory;
    protected HttpClient Client => _client;

    protected OrderIntegrationTestBase(PostgresContainerFixture postgres)
    {
        _factory = new OrderApiFactory(postgres);
        _client = _factory.CreateClient();
    }

    public virtual async Task InitializeAsync()
    {
        await _factory.InitializeAsync();
        await _factory.ResetDatabaseAsync();
    }

    public virtual async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    protected OrderDbContext CreateDbContext()
    {
        var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    }

    protected ITestHarness GetTestHarness()
    {
        return _factory.Services.GetTestHarness();
    }
}
