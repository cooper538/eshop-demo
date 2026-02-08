using EShop.Products.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EShop.Product.UnitTests.Helpers;

public sealed class TestDbContextFactory : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ProductDbContext> _options;

    public TestDbContextFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<ProductDbContext>().UseSqlite(_connection).Options;

        using var context = new ProductDbContext(_options);
        context.Database.EnsureCreated();
    }

    public ProductDbContext CreateContext() => new(_options);

    public void Dispose()
    {
        _connection.Dispose();
    }
}
