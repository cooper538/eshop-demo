using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EShop.E2E.Tests.Fixtures;

public sealed class E2ETestFixture : IAsyncLifetime
{
    private DistributedApplication? _app;

    public WireMockFixture WireMock { get; } = new();

    public HttpClient GatewayClient { get; private set; } = null!;

    public DistributedApplication App =>
        _app ?? throw new InvalidOperationException("App not started");

    public async Task InitializeAsync()
    {
        await WireMock.InitializeAsync();

        var appHost =
            await DistributedApplicationTestingBuilder.CreateAsync<Projects.EShop_AppHost>();

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
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("gateway", cts.Token);
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("product-service", cts.Token);
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("order-service", cts.Token);

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

        await WireMock.DisposeAsync();
    }

    public async Task WaitForServiceHealthyAsync(
        string resourceName,
        CancellationToken cancellationToken = default
    )
    {
        if (_app is null)
        {
            throw new InvalidOperationException("App not started");
        }

        await _app.ResourceNotifications.WaitForResourceHealthyAsync(
            resourceName,
            cancellationToken
        );
    }
}
