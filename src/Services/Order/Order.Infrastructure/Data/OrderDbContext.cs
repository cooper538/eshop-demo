using EShop.Common.Application.Data;
using EShop.Common.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Order.Application.Data;
using Order.Domain.Entities;

namespace Order.Infrastructure.Data;

public class OrderDbContext : DbContext, IOrderDbContext, IChangeTrackerAccessor, IUnitOfWork
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options) { }

    public DbSet<OrderEntity> Orders => Set<OrderEntity>();

    ChangeTracker IChangeTrackerAccessor.ChangeTracker => ChangeTracker;

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Add(_ => new RemoveEntitySuffixConvention());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);

        // MassTransit Outbox entities
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
