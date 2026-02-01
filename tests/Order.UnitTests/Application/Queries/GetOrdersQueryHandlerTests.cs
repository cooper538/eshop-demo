using EShop.Order.Application.Queries.GetOrders;
using EShop.Order.Infrastructure.Data;
using EShop.Order.UnitTests.Helpers;

namespace EShop.Order.UnitTests.Application.Queries;

public class GetOrdersQueryHandlerTests : IDisposable
{
    private readonly TestDbContextFactory _dbContextFactory;

    public GetOrdersQueryHandlerTests()
    {
        _dbContextFactory = new TestDbContextFactory();
    }

    public void Dispose()
    {
        _dbContextFactory.Dispose();
        GC.SuppressFinalize(this); // CA1816: required for IDisposable pattern
    }

    private GetOrdersQueryHandler CreateHandler(OrderDbContext context)
    {
        return new GetOrdersQueryHandler(context);
    }

    [Fact]
    public async Task Handle_ReturnsPagedResults()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        for (var i = 0; i < 5; i++)
        {
            context.Orders.Add(
                OrderTestHelper.CreateOrder(createdAt: DateTime.UtcNow.AddMinutes(-i))
            );
        }
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var handler = CreateHandler(context);
        var query = new GetOrdersQuery(CustomerId: null, Page: 1, PageSize: 3);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result
            .Should()
            .BeEquivalentTo(
                new
                {
                    TotalCount = 5,
                    TotalPages = 2,
                    Page = 1,
                },
                options => options.ExcludingMissingMembers()
            );
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithCustomerIdFilter_ReturnsOnlyMatchingOrders()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        var targetCustomerId = Guid.NewGuid();
        var otherCustomerId = Guid.NewGuid();

        context.Orders.Add(OrderTestHelper.CreateOrder(customerId: targetCustomerId));
        context.Orders.Add(OrderTestHelper.CreateOrder(customerId: targetCustomerId));
        context.Orders.Add(OrderTestHelper.CreateOrder(customerId: otherCustomerId));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var handler = CreateHandler(context);
        var query = new GetOrdersQuery(CustomerId: targetCustomerId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Should().AllSatisfy(o => o.CustomerId.Should().Be(targetCustomerId));
    }
}
