using EShop.Products.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EShop.E2E.Tests.Fixtures;

public sealed class TestProductDbContext : DbContext
{
    public TestProductDbContext(DbContextOptions<TestProductDbContext> options)
        : base(options) { }

    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    public DbSet<StockEntity> Stocks => Set<StockEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.ToTable("Product");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Ignore(e => e.DomainEvents);
        });

        modelBuilder.Entity<StockEntity>(entity =>
        {
            entity.ToTable("Stock");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProductId).IsUnique();
            entity.Ignore(e => e.Reservations);
            entity.Ignore(e => e.AvailableQuantity);
            entity.Ignore(e => e.IsLowStock);
            entity.Ignore(e => e.DomainEvents);
        });
    }
}
