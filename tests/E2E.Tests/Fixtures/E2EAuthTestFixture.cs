using System.Net.Http.Headers;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using EShop.E2E.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EShop.E2E.Tests.Fixtures;

public sealed class E2EAuthTestFixture : IAsyncLifetime
{
    private DistributedApplication? _app;

    public HttpClient GatewayClient { get; private set; } = null!;

    public DistributedApplication App =>
        _app ?? throw new InvalidOperationException("App not started");

    public async Task InitializeAsync()
    {
        var appHost =
            await DistributedApplicationTestingBuilder.CreateAsync<Projects.EShop_AppHost>([
                "--Parameters:postgres-password=test-password-e2e",
                "--Gateway:Authentication:Enabled=true",
                "--Gateway:Authentication:UseTestScheme=true",
                $"--Gateway:Authentication:TestSecretKey={MockJwtTokenGenerator.TestSecretKey}",
            ]);

        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddFilter("Aspire", LogLevel.Debug);
            logging.AddFilter("Microsoft.Hosting", LogLevel.Warning);
        });

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        await Task.WhenAll(
            _app.ResourceNotifications.WaitForResourceHealthyAsync("gateway", cts.Token),
            _app.ResourceNotifications.WaitForResourceHealthyAsync("product-service", cts.Token),
            _app.ResourceNotifications.WaitForResourceHealthyAsync("order-service", cts.Token)
        );

        GatewayClient = _app.CreateHttpClient("gateway");
    }

    public async Task DisposeAsync()
    {
        GatewayClient.Dispose();

        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }

    public HttpClient CreateAuthenticatedClient(string userId, string[]? roles = null)
    {
        var client = App.CreateHttpClient("gateway");
        var token = MockJwtTokenGenerator.GenerateToken(userId, roles);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public HttpClient CreateClientWithToken(string token)
    {
        var client = App.CreateHttpClient("gateway");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
