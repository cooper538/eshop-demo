using EShop.Common.Application.Exceptions;
using EShop.Contracts.ServiceClients.Product;
using EShop.Order.Application.Commands.CancelOrder;
using EShop.Order.Domain.Enums;
using EShop.Order.Infrastructure.Data;
using EShop.Order.UnitTests.Helpers;
using EShop.SharedKernel.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Order.UnitTests.Application.Commands;

public class CancelOrderCommandHandlerTests : IDisposable
{
    private readonly TestDbContextFactory _dbContextFactory;
    private readonly Mock<IProductServiceClient> _productClientMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly Mock<ILogger<CancelOrderCommandHandler>> _loggerMock;
    private readonly DateTime _fixedTime = new(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

    public CancelOrderCommandHandlerTests()
    {
        _dbContextFactory = new TestDbContextFactory();
        _productClientMock = new Mock<IProductServiceClient>();
        _loggerMock = new Mock<ILogger<CancelOrderCommandHandler>>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(_fixedTime);
    }

    public void Dispose()
    {
        _dbContextFactory.Dispose();
        GC.SuppressFinalize(this); // CA1816: required for IDisposable pattern
    }

    [Fact]
    public async Task Handle_WhenOrderConfirmed_CancelsSuccessfully()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        var order = OrderTestHelper.CreateConfirmedOrder();
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var handler = CreateHandler(context);
        var command = new CancelOrderCommand(order.Id, "Customer request");

        _productClientMock
            .Setup(x =>
                x.ReleaseStockAsync(It.IsAny<ReleaseStockRequest>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new StockReleaseResult(Success: true));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        result.Success.Should().BeTrue();
        result.Status.Should().Be(EOrderStatus.Cancelled.ToString());

        await using var verifyContext = _dbContextFactory.CreateContext();
        var savedOrder = await verifyContext.Orders.FirstOrDefaultAsync(o => o.Id == order.Id);
        savedOrder!.Status.Should().Be(EOrderStatus.Cancelled);
        savedOrder.RejectionReason.Should().Be("Customer request");
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ThrowsNotFoundException()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        var handler = CreateHandler(context);
        var command = new CancelOrderCommand(Guid.NewGuid(), "Reason");

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenOrderNotConfirmed_ReturnsFailure()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        var order = OrderTestHelper.CreateOrder(); // Status = Created
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var handler = CreateHandler(context);
        var command = new CancelOrderCommand(order.Id, "Reason");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Order cannot be cancelled in current status");

        await using var verifyContext = _dbContextFactory.CreateContext();
        var savedOrder = await verifyContext.Orders.FirstOrDefaultAsync(o => o.Id == order.Id);
        savedOrder!.Status.Should().Be(EOrderStatus.Created);
    }

    [Fact]
    public async Task Handle_WhenStockReleaseFails_StillCancelsOrder()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        var order = OrderTestHelper.CreateConfirmedOrder();
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var handler = CreateHandler(context);
        var command = new CancelOrderCommand(order.Id, "Customer request");

        _productClientMock
            .Setup(x =>
                x.ReleaseStockAsync(It.IsAny<ReleaseStockRequest>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                new StockReleaseResult(Success: false, FailureReason: "Stock already released")
            );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert - cancellation succeeds even if stock release fails (best-effort)
        result.Success.Should().BeTrue();

        await using var verifyContext = _dbContextFactory.CreateContext();
        var savedOrder = await verifyContext.Orders.FirstOrDefaultAsync(o => o.Id == order.Id);
        savedOrder!.Status.Should().Be(EOrderStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_WhenStockReleaseThrows_StillCancelsOrder()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        var order = OrderTestHelper.CreateConfirmedOrder();
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var handler = CreateHandler(context);
        var command = new CancelOrderCommand(order.Id, "Customer request");

        _productClientMock
            .Setup(x =>
                x.ReleaseStockAsync(It.IsAny<ReleaseStockRequest>(), It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(new InvalidOperationException("Service unavailable"));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert - cancellation succeeds even if stock release throws (best-effort)
        result.Success.Should().BeTrue();

        // New context bypasses ChangeTracker cache
        await using var verifyContext = _dbContextFactory.CreateContext();
        var savedOrder = await verifyContext.Orders.FirstOrDefaultAsync(o => o.Id == order.Id);
        savedOrder!.Status.Should().Be(EOrderStatus.Cancelled);
    }

    private CancelOrderCommandHandler CreateHandler(OrderDbContext context)
    {
        return new CancelOrderCommandHandler(
            context,
            _productClientMock.Object,
            _dateTimeProviderMock.Object,
            _loggerMock.Object
        );
    }
}
