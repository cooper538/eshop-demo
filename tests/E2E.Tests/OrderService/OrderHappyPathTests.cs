using System.Net;
using System.Net.Http.Json;
using EShop.E2E.Tests.Fixtures;
using static EShop.E2E.Tests.OrderService.OrderTestHelpers;

namespace EShop.E2E.Tests.OrderService;

public class OrderHappyPathTests : E2ETestBase
{
    public OrderHappyPathTests(E2ETestFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task CreateOrder_WithValidData_ReturnsConfirmed()
    {
        var product = await GatewayClient.GetFirstAvailableProductAsync();
        var request = CreateOrderRequest(product.Id, product.Name, product.Price, quantity: 1);

        var response = await GatewayClient.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateOrderResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.OrderId.Should().NotBeEmpty();
        result.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task GetOrder_AfterCreation_ReturnsOrderDetails()
    {
        var product = await GatewayClient.GetFirstAvailableProductAsync();
        var createRequest = CreateOrderRequest(
            product.Id,
            product.Name,
            product.Price,
            quantity: 2
        );
        var createResponse = await GatewayClient.PostAsJsonAsync("/api/orders", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateOrderResponse>(
            JsonOptions
        );

        var response = await GatewayClient.GetAsync($"/api/orders/{created!.OrderId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var order = await response.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        order.Should().NotBeNull();
        order!.Id.Should().Be(created.OrderId);
        order.Status.Should().Be("Confirmed");
        order.CustomerEmail.Should().Be("test@example.com");
        order.Items.Should().HaveCount(1);
        order.Items[0].Quantity.Should().Be(2);
    }

    [Fact]
    public async Task ListOrders_ByCustomerId_ReturnsOnlyCustomerOrders()
    {
        var customerId = Guid.NewGuid();
        var product = await GatewayClient.GetFirstAvailableProductAsync();

        for (var i = 0; i < 3; i++)
        {
            var request = CreateOrderRequest(
                product.Id,
                product.Name,
                product.Price,
                quantity: 1,
                customerId: customerId
            );
            await GatewayClient.PostAsJsonAsync("/api/orders", request);
        }

        var response = await GatewayClient.GetAsync(
            $"/api/orders?customerId={customerId}&page=1&pageSize=10"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<GetOrdersResponse>(JsonOptions);
        result.Should().NotBeNull();
        result
            .Should()
            .BeEquivalentTo(
                new
                {
                    TotalCount = 3,
                    Page = 1,
                    PageSize = 10,
                },
                options => options.ExcludingMissingMembers()
            );
        result!.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task CancelOrder_ConfirmedOrder_ReturnsCancelled()
    {
        var product = await GatewayClient.GetFirstAvailableProductAsync();
        var createRequest = CreateOrderRequest(
            product.Id,
            product.Name,
            product.Price,
            quantity: 1
        );
        var createResponse = await GatewayClient.PostAsJsonAsync("/api/orders", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateOrderResponse>(
            JsonOptions
        );

        var cancelRequest = new { Reason = "Changed my mind" };
        var response = await GatewayClient.PostAsJsonAsync(
            $"/api/orders/{created!.OrderId}/cancel",
            cancelRequest
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CancelOrderResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Status.Should().Be("Cancelled");
    }
}
