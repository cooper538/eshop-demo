using EShop.Common.Application.Exceptions;
using EShop.Order.Application.Queries.GetOrderById;
using EShop.Order.Infrastructure.Data;
using EShop.Order.UnitTests.Helpers;

namespace EShop.Order.UnitTests.Application.Queries;

public class GetOrderByIdQueryHandlerTests : IDisposable
{
    private readonly TestDbContextFactory _dbContextFactory;

    public GetOrderByIdQueryHandlerTests()
    {
        _dbContextFactory = new TestDbContextFactory();
    }

    public void Dispose()
    {
        _dbContextFactory.Dispose();
        GC.SuppressFinalize(this); // CA1816: required for IDisposable pattern
    }

    private GetOrderByIdQueryHandler CreateHandler(OrderDbContext context)
    {
        return new GetOrderByIdQueryHandler(context);
    }

    [Fact]
    public async Task Handle_WhenOrderExists_ReturnsOrderDto()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        var order = OrderTestHelper.CreateOrder(
            customerId: Guid.NewGuid(),
            customerEmail: "test@example.com"
        );
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var handler = CreateHandler(context);
        var query = new GetOrderByIdQuery(order.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(order.Id);
        result.CustomerEmail.Should().Be("test@example.com");
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ThrowsNotFoundException()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        var handler = CreateHandler(context);
        var query = new GetOrderByIdQuery(Guid.NewGuid());

        // Act
        var act = () => handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
