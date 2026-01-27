using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Products.Domain.Entities;

namespace Products.Application.Data;

public interface IProductDbContext
{
    DbSet<ProductEntity> Products { get; }
    DbSet<StockEntity> Stocks { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default
    );
}
