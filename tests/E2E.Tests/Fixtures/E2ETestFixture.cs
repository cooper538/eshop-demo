using System.Net.Http.Json;
using System.Text.Json;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EShop.E2E.Tests.Fixtures;

public sealed class E2ETestFixture : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private DistributedApplication? _app;
    private ProductDto[]? _cachedProducts;

    public HttpClient GatewayClient { get; private set; } = null!;

    public DistributedApplication App =>
        _app ?? throw new InvalidOperationException("App not started");

    public async Task InitializeAsync()
    {
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

    public async Task<ProductDto> GetCachedProductAsync()
    {
        if (_cachedProducts is null)
        {
            var response = await GatewayClient.GetAsync("/api/products");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GetProductsResponse>(JsonOptions);
            _cachedProducts =
                result?.Items.ToArray()
                ?? throw new InvalidOperationException("No products available");
        }

        return _cachedProducts.First();
    }

    public async Task<ProductDto[]> GetCachedProductsAsync()
    {
        if (_cachedProducts is null)
        {
            var response = await GatewayClient.GetAsync("/api/products");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GetProductsResponse>(JsonOptions);
            _cachedProducts =
                result?.Items.ToArray()
                ?? throw new InvalidOperationException("No products available");
        }

        return _cachedProducts;
    }
}
