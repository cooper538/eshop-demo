using EShop.Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EShop.Order.Application.Data;

public interface IOrderDbContext
{
    DbSet<OrderEntity> Orders { get; }
}
