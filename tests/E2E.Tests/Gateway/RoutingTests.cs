using System.Net;
using EShop.E2E.Tests.Fixtures;

namespace EShop.E2E.Tests.Gateway;

public class GatewayRoutingTests : E2ETestBase
{
    public GatewayRoutingTests(E2ETestFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task Gateway_OrdersEndpoint_ReachesOrderService()
    {
        var response = await GatewayClient.GetAsync("/api/orders");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Gateway_ProductsEndpoint_ReachesProductService()
    {
        var response = await GatewayClient.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
