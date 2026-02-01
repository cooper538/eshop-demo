using System.Net;
using EShop.E2E.Tests.Fixtures;

namespace EShop.E2E.Tests.Gateway;

public class CorrelationIdTests : E2ETestBase
{
    public CorrelationIdTests(E2ETestFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task Request_WithCorrelationId_PropagatesInResponse()
    {
        var correlationId = Guid.NewGuid().ToString();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/orders");
        request.Headers.Add("X-Correlation-ID", correlationId);

        var response = await GatewayClient.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.TryGetValues("X-Correlation-ID", out var values).Should().BeTrue();
        values.Should().Contain(correlationId);
    }

    [Fact]
    public async Task Request_WithoutCorrelationId_GeneratesNew()
    {
        var response = await GatewayClient.GetAsync("/api/orders");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.TryGetValues("X-Correlation-ID", out var values).Should().BeTrue();
        values.Should().NotBeNullOrEmpty();
        Guid.TryParse(values!.First(), out _).Should().BeTrue();
    }
}
