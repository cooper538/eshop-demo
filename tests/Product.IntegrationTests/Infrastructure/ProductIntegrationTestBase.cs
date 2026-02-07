using EShop.Products.Infrastructure.Data;
using EShop.Products.IntegrationTests.Fixtures;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Products.IntegrationTests.Infrastructure;

[Collection(ProductIntegrationTestCollection.Name)]
#pragma warning disable CA1001 // IAsyncLifetime handles disposal via DisposeAsync
public abstract class ProductIntegrationTestBase : IAsyncLifetime
{
    private readonly ProductApiFactory _factory;
    private readonly HttpClient _client;

    protected ProductApiFactory Factory => _factory;
    protected HttpClient Client => _client;

    protected ProductIntegrationTestBase(PostgresContainerFixture postgres)
    {
        _factory = new ProductApiFactory(postgres);
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

    protected ProductDbContext CreateDbContext()
    {
        var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    }

    protected ITestHarness GetTestHarness()
    {
        return _factory.Services.GetTestHarness();
    }
}
