using EShop.Products.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EShop.Products.Application.Data;

public interface IProductDbContext
{
    DbSet<ProductEntity> Products { get; }
    DbSet<StockEntity> Stocks { get; }

    Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default
    );
}
