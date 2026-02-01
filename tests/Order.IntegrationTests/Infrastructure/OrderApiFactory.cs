using EShop.Contracts.ServiceClients.Product;
using EShop.Order.Infrastructure.Data;
using EShop.Order.IntegrationTests.Fixtures;
using EShop.ServiceDefaults;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Moq;

namespace EShop.Order.IntegrationTests.Infrastructure;

public class OrderApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgresContainerFixture _postgres;
    private readonly DatabaseFixture _database;
    private readonly Mock<IProductServiceClient> _productServiceMock = new();

    public Mock<IProductServiceClient> ProductServiceMock => _productServiceMock;

    public OrderApiFactory(PostgresContainerFixture postgres)
    {
        _postgres = postgres;
        _database = new DatabaseFixture();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Override connection string to use Testcontainer instead of real database
        builder.UseSetting(
            $"ConnectionStrings:{ResourceNames.Databases.Order}",
            _postgres.ConnectionString
        );

        builder.ConfigureTestServices(services =>
        {
            // Remove duplicate MassTransit health checks before test harness adds its own
            services.AddSingleton<
                IConfigureOptions<HealthCheckServiceOptions>,
                RemoveMassTransitHealthChecks
            >();

            // Replace MassTransit with test harness (in-memory transport)
            services.AddMassTransitTestHarness(cfg =>
            {
                cfg.SetTestTimeouts(TimeSpan.FromSeconds(30));
            });

            // Replace ProductServiceClient with mock for resilience testing
            services.RemoveAll<IProductServiceClient>();
            services.AddSingleton(_productServiceMock.Object);
        });
    }

    public async Task InitializeAsync()
    {
        // Apply migrations
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        await db.Database.MigrateAsync();

        // Initialize Respawn
        _database.SetConnectionString(_postgres.ConnectionString);
        await _database.InitializeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await _database.ResetAsync();
    }

    public new async Task DisposeAsync()
    {
        await _database.DisposeAsync();
        await base.DisposeAsync();
    }

    /// <summary>
    /// Removes MassTransit health checks to avoid duplicates when using TestHarness.
    /// See: https://github.com/MassTransit/MassTransit/discussions/4498
    /// </summary>
    private sealed class RemoveMassTransitHealthChecks
        : IConfigureOptions<HealthCheckServiceOptions>
    {
        public void Configure(HealthCheckServiceOptions options)
        {
            var massTransitChecks = options
                .Registrations.Where(x => x.Tags.Contains("masstransit"))
                .ToList();

            foreach (var check in massTransitChecks)
            {
                options.Registrations.Remove(check);
            }
        }
    }
}
