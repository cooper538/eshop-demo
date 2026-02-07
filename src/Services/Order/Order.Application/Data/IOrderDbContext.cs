using EShop.Order.Domain.Entities;
using EShop.Order.Domain.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace EShop.Order.Application.Data;

public interface IOrderDbContext
{
    DbSet<OrderEntity> Orders { get; }
    DbSet<ProductSnapshot> ProductSnapshots { get; }
}
