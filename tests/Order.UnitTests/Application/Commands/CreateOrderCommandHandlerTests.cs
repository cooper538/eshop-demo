using EShop.Common.Application.Exceptions;
using EShop.Contracts.ServiceClients;
using EShop.Contracts.ServiceClients.Product;
using EShop.Order.Application.Commands.CreateOrder;
using EShop.Order.Domain.Enums;
using EShop.Order.Domain.ReadModels;
using EShop.Order.Infrastructure.Data;
using EShop.Order.UnitTests.Helpers;
using EShop.SharedKernel.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Order.UnitTests.Application.Commands;

public class CreateOrderCommandHandlerTests : IDisposable
{
    private readonly TestDbContextFactory _dbContextFactory;
    private readonly Mock<IProductServiceClient> _productClientMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly Mock<ILogger<CreateOrderCommandHandler>> _loggerMock;
    private readonly DateTime _fixedTime = new(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

    public CreateOrderCommandHandlerTests()
    {
        _dbContextFactory = new TestDbContextFactory();
        _productClientMock = new Mock<IProductServiceClient>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _loggerMock = new Mock<ILogger<CreateOrderCommandHandler>>();
        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(_fixedTime);
    }

    public void Dispose()
    {
        _dbContextFactory.Dispose();
        GC.SuppressFinalize(this);
    }

    private CreateOrderCommandHandler CreateHandler(OrderDbContext context)
    {
        return new CreateOrderCommandHandler(
            context,
            _productClientMock.Object,
            _dateTimeProviderMock.Object,
            _loggerMock.Object
        );
    }

    private async Task SeedProductSnapshotAsync(
        OrderDbContext context,
        Guid productId,
        string productName = "Test Product",
        decimal price = 50.00m
    )
    {
        context.ProductSnapshots.Add(
            ProductSnapshot.Create(productId, productName, price, _fixedTime)
        );
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenStockAvailable_ReturnsConfirmedOrder()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        var handler = CreateHandler(context);
        var productId = Guid.NewGuid();
        var command = CreateValidCommand(productId);

        await SeedProductSnapshotAsync(context, productId);
        _productClientMock
            .Setup(x =>
                x.ReserveStockAsync(It.IsAny<ReserveStockRequest>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new StockReservationResult(Success: true));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(EOrderStatus.Confirmed.ToString());
        result.OrderId.Should().NotBeEmpty();
        result.Message.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenStockAvailable_PersistsConfirmedOrder()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        var handler = CreateHandler(context);
        var productId = Guid.NewGuid();
        var command = CreateValidCommand(productId);

        await SeedProductSnapshotAsync(context, productId);
        _productClientMock
            .Setup(x =>
                x.ReserveStockAsync(It.IsAny<ReserveStockRequest>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new StockReservationResult(Success: true));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        await using var verifyContext = _dbContextFactory.CreateContext();
        var savedOrder = await verifyContext
            .Orders.Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == result.OrderId);

        savedOrder.Should().NotBeNull();
        savedOrder!.Status.Should().Be(EOrderStatus.Confirmed);
        savedOrder.CustomerEmail.Should().Be(command.CustomerEmail);
        savedOrder.Items.Should().HaveCount(1);
        savedOrder.Items[0].ProductName.Should().Be("Test Product");
        savedOrder.Items[0].UnitPrice.Should().Be(50.00m);
    }

    [Fact]
    public async Task Handle_WhenStockUnavailable_ReturnsRejectedOrder()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        var handler = CreateHandler(context);
        var productId = Guid.NewGuid();
        var command = CreateValidCommand(productId);
        const string reason = "Insufficient stock for Product A";

        await SeedProductSnapshotAsync(context, productId);
        _productClientMock
            .Setup(x =>
                x.ReserveStockAsync(It.IsAny<ReserveStockRequest>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                new StockReservationResult(
                    Success: false,
                    FailureReason: reason,
                    ErrorCode: EStockReservationErrorCode.InsufficientStock
                )
            );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        result.Status.Should().Be(EOrderStatus.Rejected.ToString());
        result.Message.Should().Be(reason);

        await using var verifyContext = _dbContextFactory.CreateContext();
        var savedOrder = await verifyContext.Orders.FirstOrDefaultAsync(o =>
            o.Id == result.OrderId
        );
        savedOrder!.Status.Should().Be(EOrderStatus.Rejected);
    }

    [Fact]
    public async Task Handle_WhenProductNotFoundInSnapshot_ThrowsValidationException()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        var handler = CreateHandler(context);
        var productId = Guid.NewGuid();
        var command = CreateValidCommand(productId);

        // Do NOT seed ProductSnapshot — product not in local catalog

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>().WithMessage("Products not found:*");
    }

    [Fact]
    public async Task Handle_WhenProductNotFoundInReserveStock_ThrowsValidationException()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        var handler = CreateHandler(context);
        var productId = Guid.NewGuid();
        var command = CreateValidCommand(productId);

        await SeedProductSnapshotAsync(context, productId);
        _productClientMock
            .Setup(x =>
                x.ReserveStockAsync(It.IsAny<ReserveStockRequest>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                new StockReservationResult(
                    Success: false,
                    FailureReason: "Product not found",
                    ErrorCode: EStockReservationErrorCode.ProductNotFound
                )
            );

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenReserveStockServiceUnavailable_ReturnsCreatedOrderWithError()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        var handler = CreateHandler(context);
        var productId = Guid.NewGuid();
        var command = CreateValidCommand(productId);
        const string errorMessage = "Service unavailable";

        await SeedProductSnapshotAsync(context, productId);
        _productClientMock
            .Setup(x =>
                x.ReserveStockAsync(It.IsAny<ReserveStockRequest>(), It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(
                new ServiceClientException(
                    errorMessage,
                    null,
                    EServiceClientErrorCode.ServiceUnavailable
                )
            );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        result.Status.Should().Be(EOrderStatus.Created.ToString());
        result.Message.Should().Be("Stock reservation pending");

        await using var verifyContext = _dbContextFactory.CreateContext();
        var savedOrder = await verifyContext.Orders.FirstOrDefaultAsync(o =>
            o.Id == result.OrderId
        );
        savedOrder!.Status.Should().Be(EOrderStatus.Created);
    }

    private static CreateOrderCommand CreateValidCommand(Guid? productId = null)
    {
        return new CreateOrderCommand(
            Guid.NewGuid(),
            "customer@example.com",
            [new CreateOrderItemDto(productId ?? Guid.NewGuid(), 2)]
        );
    }
}
