using EShop.Products.Infrastructure.Data;
using EShop.Products.IntegrationTests.Fixtures;
using EShop.ServiceDefaults;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace EShop.Products.IntegrationTests.Infrastructure;

public class ProductApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgresContainerFixture _postgres;
    private readonly DatabaseFixture _database;

    public ProductApiFactory(PostgresContainerFixture postgres)
    {
        _postgres = postgres;
        _database = new DatabaseFixture();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting(
            $"ConnectionStrings:{ResourceNames.Databases.Product}",
            _postgres.ConnectionString
        );

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<
                IConfigureOptions<HealthCheckServiceOptions>,
                RemoveMassTransitHealthChecks
            >();

            services.AddMassTransitTestHarness(cfg =>
            {
                cfg.SetTestTimeouts(TimeSpan.FromSeconds(30));
            });
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        await db.Database.MigrateAsync();

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
