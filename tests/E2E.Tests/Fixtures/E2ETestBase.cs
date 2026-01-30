namespace EShop.E2E.Tests.Fixtures;

[Collection(E2ETestCollection.Name)]
public abstract class E2ETestBase : IAsyncLifetime
{
    protected E2ETestFixture Fixture { get; }

    protected HttpClient GatewayClient => Fixture.GatewayClient;

    protected WireMockFixture WireMock => Fixture.WireMock;

    protected E2ETestBase(E2ETestFixture fixture)
    {
        Fixture = fixture;
    }

    public virtual Task InitializeAsync()
    {
        WireMock.Reset();
        return Task.CompletedTask;
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected async Task<T> WaitForAsync<T>(
        Func<Task<T>> operation,
        Func<T, bool> predicate,
        TimeSpan? timeout = null,
        TimeSpan? pollingInterval = null
    )
    {
        timeout ??= TimeSpan.FromSeconds(30);
        pollingInterval ??= TimeSpan.FromMilliseconds(500);

        using var cts = new CancellationTokenSource(timeout.Value);

        while (!cts.Token.IsCancellationRequested)
        {
            var result = await operation();

            if (predicate(result))
            {
                return result;
            }

            await Task.Delay(pollingInterval.Value, cts.Token);
        }

        throw new TimeoutException(
            $"Operation did not complete within {timeout.Value.TotalSeconds} seconds"
        );
    }

    protected async Task WaitForConditionAsync(
        Func<Task<bool>> condition,
        TimeSpan? timeout = null,
        TimeSpan? pollingInterval = null,
        string? message = null
    )
    {
        timeout ??= TimeSpan.FromSeconds(30);
        pollingInterval ??= TimeSpan.FromMilliseconds(500);

        using var cts = new CancellationTokenSource(timeout.Value);

        while (!cts.Token.IsCancellationRequested)
        {
            if (await condition())
            {
                return;
            }

            await Task.Delay(pollingInterval.Value, cts.Token);
        }

        throw new TimeoutException(
            message ?? $"Condition was not met within {timeout.Value.TotalSeconds} seconds"
        );
    }

    protected async Task<HttpResponseMessage> GetWithRetryAsync(
        string requestUri,
        int maxRetries = 3,
        TimeSpan? delay = null
    )
    {
        delay ??= TimeSpan.FromSeconds(1);

        for (var i = 0; i < maxRetries; i++)
        {
            var response = await GatewayClient.GetAsync(requestUri);

            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            if (i < maxRetries - 1)
            {
                await Task.Delay(delay.Value);
            }
        }

        return await GatewayClient.GetAsync(requestUri);
    }
}
