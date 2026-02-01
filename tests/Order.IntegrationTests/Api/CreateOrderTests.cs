using System.Net;
using System.Net.Http.Json;
using EShop.Contracts.ServiceClients;
using EShop.Contracts.ServiceClients.Product;
using EShop.Order.Application.Commands.CreateOrder;
using EShop.Order.IntegrationTests.Fixtures;
using EShop.Order.IntegrationTests.Infrastructure;
using Moq;

namespace EShop.Order.IntegrationTests.Api;

public class CreateOrderTests : OrderIntegrationTestBase
{
    public CreateOrderTests(PostgresContainerFixture postgres)
        : base(postgres) { }

    [Fact]
    public async Task CreateOrder_ProductServiceUnavailable_ReturnsCreatedWithMessage()
    {
        // Arrange - mock throws ServiceClientException (service unavailable)
        Factory
            .ProductServiceMock.Setup(x =>
                x.ReserveStockAsync(It.IsAny<ReserveStockRequest>(), It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(
                new ServiceClientException(
                    "Service unavailable",
                    null,
                    EServiceClientErrorCode.ServiceUnavailable
                )
            );

        var request = CreateOrderRequest();

        // Act
        var response = await Client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateOrderResult>();
        result.Should().NotBeNull();
        result!.Status.Should().Be("Created");
        result.Message.Should().Be("Stock reservation pending");
    }

    [Fact]
    public async Task CreateOrder_ProductServiceTimeout_ReturnsCreatedWithMessage()
    {
        // Arrange - mock throws ServiceClientException (timeout)
        Factory
            .ProductServiceMock.Setup(x =>
                x.ReserveStockAsync(It.IsAny<ReserveStockRequest>(), It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(
                new ServiceClientException(
                    "Deadline exceeded",
                    null,
                    EServiceClientErrorCode.Timeout
                )
            );

        var request = CreateOrderRequest();

        // Act
        var response = await Client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateOrderResult>();
        result.Should().NotBeNull();
        result!.Status.Should().Be("Created");
        result.Message.Should().Be("Stock reservation pending");
    }

    [Fact]
    public async Task CreateOrder_InsufficientStock_ReturnsCreatedWithRejectedStatus()
    {
        // Arrange - mock returns insufficient stock
        Factory
            .ProductServiceMock.Setup(x =>
                x.ReserveStockAsync(It.IsAny<ReserveStockRequest>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                new StockReservationResult(
                    false,
                    "Not enough stock for Product A",
                    EStockReservationErrorCode.InsufficientStock
                )
            );

        var request = CreateOrderRequest();

        // Act
        var response = await Client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateOrderResult>();
        result.Should().NotBeNull();
        result!.Status.Should().Be("Rejected");
        result.Message.Should().Contain("Not enough stock");
    }

    [Fact]
    public async Task CreateOrder_ProductNotFound_ReturnsBadRequest()
    {
        // Arrange - mock returns product not found (validation error)
        Factory
            .ProductServiceMock.Setup(x =>
                x.ReserveStockAsync(It.IsAny<ReserveStockRequest>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                new StockReservationResult(
                    false,
                    "Product not found",
                    EStockReservationErrorCode.ProductNotFound
                )
            );

        var request = CreateOrderRequest();

        // Act
        var response = await Client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_StockReserved_ReturnsCreatedWithConfirmedStatus()
    {
        // Arrange - mock returns success
        Factory
            .ProductServiceMock.Setup(x =>
                x.ReserveStockAsync(It.IsAny<ReserveStockRequest>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new StockReservationResult(true));

        var request = CreateOrderRequest();

        // Act
        var response = await Client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateOrderResult>();
        result.Should().NotBeNull();
        result!.Status.Should().Be("Confirmed");
        result.Message.Should().BeNull();
    }

    private static CreateOrderCommand CreateOrderRequest() =>
        new(
            Guid.NewGuid(),
            "resilience-test@example.com",
            [new CreateOrderItemDto(Guid.NewGuid(), "Test Product", 1, 99.99m)]
        );
}
