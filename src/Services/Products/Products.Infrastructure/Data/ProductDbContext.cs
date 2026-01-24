using EShop.Common.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Products.Application.Data;
using Products.Domain.Entities;

namespace Products.Infrastructure.Data;

public class ProductDbContext : DbContext, IProductDbContext, IChangeTrackerAccessor
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options) { }

    public DbSet<ProductEntity> Products => Set<ProductEntity>();

    ChangeTracker IChangeTrackerAccessor.ChangeTracker => ChangeTracker;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductDbContext).Assembly);
    }
}
