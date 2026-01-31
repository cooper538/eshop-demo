using EShop.Common.Application.Data;
using EShop.Common.Infrastructure.Data;
using EShop.Products.Application.Data;
using EShop.Products.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace EShop.Products.Infrastructure.Data;

public class ProductDbContext : DbContext, IProductDbContext, IChangeTrackerAccessor, IUnitOfWork
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options) { }

    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    public DbSet<StockEntity> Stocks => Set<StockEntity>();

    ChangeTracker IChangeTrackerAccessor.ChangeTracker => ChangeTracker;

    public Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default
    ) => Database.BeginTransactionAsync(cancellationToken);

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Add(_ => new RemoveEntitySuffixConvention());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductDbContext).Assembly);

        // MassTransit Outbox entities
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
