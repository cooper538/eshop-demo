using Microsoft.EntityFrameworkCore;
using Products.Domain.Entities;

namespace Products.Application.Data;

public interface IProductDbContext
{
    DbSet<ProductEntity> Products { get; }
    DbSet<StockEntity> Stocks { get; }
    DbSet<StockReservationEntity> StockReservations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
