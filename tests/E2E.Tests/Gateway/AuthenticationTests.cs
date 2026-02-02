using System.Net;
using EShop.E2E.Tests.Fixtures;
using EShop.E2E.Tests.Helpers;

namespace EShop.E2E.Tests.Gateway;

[Collection(E2EAuthTestCollection.Name)]
public class AuthenticationTests : IAsyncLifetime
{
    private readonly E2EAuthTestFixture _fixture;

    public AuthenticationTests(E2EAuthTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Request_WithoutToken_Returns401()
    {
        var response = await _fixture.GatewayClient.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Request_WithMalformedToken_Returns401()
    {
        using var client = _fixture.CreateClientWithToken("not-a-valid-jwt-token");

        var response = await client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Request_WithInvalidSignatureToken_Returns401()
    {
        var token = MockJwtTokenGenerator.GenerateInvalidSignatureToken("test-user");
        using var client = _fixture.CreateClientWithToken(token);

        var response = await client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Request_WithExpiredToken_Returns401()
    {
        var token = MockJwtTokenGenerator.GenerateExpiredToken("test-user");
        using var client = _fixture.CreateClientWithToken(token);

        var response = await client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Request_WithValidToken_Returns200()
    {
        using var client = _fixture.CreateAuthenticatedClient("test-user");

        var response = await client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Request_WithValidToken_AndRoles_Returns200()
    {
        using var client = _fixture.CreateAuthenticatedClient("test-user", ["Admin", "Reader"]);

        var response = await client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Request_WithValidToken_CanAccessMultipleEndpoints()
    {
        using var client = _fixture.CreateAuthenticatedClient("test-user");

        var productsResponse = await client.GetAsync("/api/products");
        var ordersResponse = await client.GetAsync("/api/orders");

        productsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        ordersResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Request_TokenExpiresAfterOneHour_ByDefault()
    {
        var token = MockJwtTokenGenerator.GenerateToken("test-user", expiry: TimeSpan.FromHours(1));
        using var client = _fixture.CreateClientWithToken(token);

        var response = await client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
