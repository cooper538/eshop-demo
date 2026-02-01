using System.Net;
using System.Net.Http.Json;
using EShop.E2E.Tests.Fixtures;
using static EShop.E2E.Tests.OrderService.OrderTestHelpers;

namespace EShop.E2E.Tests.OrderService;

public class OrderUnhappyPathTests : E2ETestBase
{
    public OrderUnhappyPathTests(E2ETestFixture fixture)
        : base(fixture) { }

    #region Validation Errors

    [Fact]
    public async Task CreateOrder_EmptyItems_Returns400()
    {
        var request = new
        {
            CustomerId = Guid.NewGuid(),
            CustomerEmail = "test@example.com",
            Items = Array.Empty<object>(),
        };

        var response = await GatewayClient.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_InvalidEmail_Returns400()
    {
        var product = await GatewayClient.GetFirstAvailableProductAsync();
        var request = new
        {
            CustomerId = Guid.NewGuid(),
            CustomerEmail = "invalid-email",
            Items = new[]
            {
                new
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = 1,
                    UnitPrice = product.Price,
                },
            },
        };

        var response = await GatewayClient.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_NonExistentProduct_Returns400()
    {
        var request = CreateOrderRequest(
            productId: Guid.NewGuid(),
            productName: "Non-existent Product",
            unitPrice: 99.99m,
            quantity: 1
        );

        var response = await GatewayClient.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Not Found

    [Fact]
    public async Task GetOrder_NonExistent_Returns404()
    {
        var nonExistentId = Guid.NewGuid();

        var response = await GatewayClient.GetAsync($"/api/orders/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CancelOrder_NonExistent_Returns404()
    {
        var nonExistentId = Guid.NewGuid();
        var cancelRequest = new { Reason = "Cancel non-existent" };

        var response = await GatewayClient.PostAsJsonAsync(
            $"/api/orders/{nonExistentId}/cancel",
            cancelRequest
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Business Rule Violations

    [Fact]
    public async Task CreateOrder_InsufficientStock_ReturnsRejected()
    {
        var product = await GatewayClient.GetFirstAvailableProductAsync();
        var request = CreateOrderRequest(product.Id, product.Name, product.Price, quantity: 99999);

        var response = await GatewayClient.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateOrderResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Status.Should().Be("Rejected");
        result.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CancelOrder_AlreadyCancelled_ReturnsBadRequest()
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

        var cancelRequest = new { Reason = "First cancel" };
        await GatewayClient.PostAsJsonAsync(
            $"/api/orders/{created!.OrderId}/cancel",
            cancelRequest
        );

        var secondCancelRequest = new { Reason = "Second cancel" };
        var response = await GatewayClient.PostAsJsonAsync(
            $"/api/orders/{created.OrderId}/cancel",
            secondCancelRequest
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CancelOrder_RejectedOrder_ReturnsBadRequest()
    {
        var product = await GatewayClient.GetFirstAvailableProductAsync();
        var createRequest = CreateOrderRequest(
            product.Id,
            product.Name,
            product.Price,
            quantity: 99999
        );
        var createResponse = await GatewayClient.PostAsJsonAsync("/api/orders", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateOrderResponse>(
            JsonOptions
        );

        created!.Status.Should().Be("Rejected");

        var cancelRequest = new { Reason = "Cancel rejected order" };
        var response = await GatewayClient.PostAsJsonAsync(
            $"/api/orders/{created.OrderId}/cancel",
            cancelRequest
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}
