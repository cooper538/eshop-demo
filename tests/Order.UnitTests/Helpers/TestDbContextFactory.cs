using EShop.Order.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EShop.Order.UnitTests.Helpers;

public sealed class TestDbContextFactory : IDisposable
{
    private readonly DbContextOptions<OrderDbContext> _options;
    private readonly string _databaseName;

    public TestDbContextFactory()
    {
        _databaseName = $"TestDb_{Guid.NewGuid()}";

        _options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;
    }

    public OrderDbContext CreateContext()
    {
        return new OrderDbContext(_options);
    }

    public void Dispose()
    {
        // InMemory database is automatically cleaned up when all references are gone
        GC.SuppressFinalize(this);
    }
}
