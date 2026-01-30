using System.Net;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace EShop.E2E.Tests.Fixtures;

public sealed class WireMockFixture : IAsyncLifetime
{
    private WireMockServer? _server;

    public string BaseUrl =>
        _server?.Url ?? throw new InvalidOperationException("WireMock server not started");

    public WireMockServer Server =>
        _server ?? throw new InvalidOperationException("WireMock server not started");

    public Task InitializeAsync()
    {
        _server = WireMockServer.Start();
        SetupDefaultMocks();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _server?.Stop();
        _server?.Dispose();
        return Task.CompletedTask;
    }

    public void Reset()
    {
        _server?.Reset();
        SetupDefaultMocks();
    }

    private void SetupDefaultMocks()
    {
        SetupSendGridMock();
    }

    private void SetupSendGridMock()
    {
        _server
            ?.Given(Request.Create().WithPath("/v3/mail/send").UsingPost())
            .RespondWith(
                Response
                    .Create()
                    .WithStatusCode(HttpStatusCode.Accepted)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("{}")
            );
    }

    public void SetupSendGridError(HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    {
        _server?.Reset();
        _server
            ?.Given(Request.Create().WithPath("/v3/mail/send").UsingPost())
            .RespondWith(
                Response
                    .Create()
                    .WithStatusCode(statusCode)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("{\"errors\":[{\"message\":\"Service unavailable\"}]}")
            );
    }

    public void SetupSendGridRateLimit()
    {
        _server?.Reset();
        _server
            ?.Given(Request.Create().WithPath("/v3/mail/send").UsingPost())
            .RespondWith(
                Response
                    .Create()
                    .WithStatusCode(HttpStatusCode.TooManyRequests)
                    .WithHeader("Content-Type", "application/json")
                    .WithHeader("Retry-After", "60")
                    .WithBody("{\"errors\":[{\"message\":\"Rate limit exceeded\"}]}")
            );
    }
}
