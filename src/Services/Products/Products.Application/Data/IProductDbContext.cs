using Microsoft.EntityFrameworkCore;
using Products.Domain.Entities;

namespace Products.Application.Data;

public interface IProductDbContext
{
    DbSet<Product> Products { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
