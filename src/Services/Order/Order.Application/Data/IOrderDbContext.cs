using Microsoft.EntityFrameworkCore;
using Order.Domain.Entities;

namespace Order.Application.Data;

public interface IOrderDbContext
{
    DbSet<OrderEntity> Orders { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
