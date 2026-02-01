using EShop.Common.Application.Exceptions;
using EShop.Contracts.ServiceClients;
using EShop.Contracts.ServiceClients.Product;
using EShop.Order.Application.Commands.CreateOrder;
using EShop.Order.Domain.Enums;
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
        GC.SuppressFinalize(this); // CA1816: required for IDisposable pattern
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

    [Fact]
    public async Task Handle_WhenStockAvailable_ReturnsConfirmedOrder()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        var handler = CreateHandler(context);
        var command = CreateValidCommand();

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
        var command = CreateValidCommand();

        _productClientMock
            .Setup(x =>
                x.ReserveStockAsync(It.IsAny<ReserveStockRequest>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new StockReservationResult(Success: true));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert - new context bypasses ChangeTracker cache, reads from actual DB
        await using var verifyContext = _dbContextFactory.CreateContext();
        var savedOrder = await verifyContext
            .Orders.Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == result.OrderId);

        savedOrder.Should().NotBeNull();
        savedOrder!.Status.Should().Be(EOrderStatus.Confirmed);
        savedOrder.CustomerEmail.Should().Be(command.CustomerEmail);
        savedOrder.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WhenStockUnavailable_ReturnsRejectedOrder()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        var handler = CreateHandler(context);
        var command = CreateValidCommand();
        const string reason = "Insufficient stock for Product A";

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

        // New context bypasses ChangeTracker cache
        await using var verifyContext = _dbContextFactory.CreateContext();
        var savedOrder = await verifyContext.Orders.FirstOrDefaultAsync(o =>
            o.Id == result.OrderId
        );
        savedOrder!.Status.Should().Be(EOrderStatus.Rejected);
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ThrowsValidationException()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        var handler = CreateHandler(context);
        var command = CreateValidCommand();

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
    public async Task Handle_WhenServiceClientThrows_ReturnsCreatedOrderWithError()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        var handler = CreateHandler(context);
        var command = CreateValidCommand();
        const string errorMessage = "Service unavailable";

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

        // New context bypasses ChangeTracker cache
        await using var verifyContext = _dbContextFactory.CreateContext();
        var savedOrder = await verifyContext.Orders.FirstOrDefaultAsync(o =>
            o.Id == result.OrderId
        );
        savedOrder!.Status.Should().Be(EOrderStatus.Created);
    }

    private static CreateOrderCommand CreateValidCommand()
    {
        return new CreateOrderCommand(
            Guid.NewGuid(),
            "customer@example.com",
            [new CreateOrderItemDto(Guid.NewGuid(), "Test Product", 2, 50.00m)]
        );
    }
}
