using EShop.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Common.Infrastructure.Data;

public abstract class AggregateRootConfiguration<TAggregate> : EntityConfiguration<TAggregate>
    where TAggregate : AggregateRoot
{
    protected override void ConfigureEntity(EntityTypeBuilder<TAggregate> builder)
    {
        // Version is manually incremented by aggregate when state changes
        // This enables proper DDD-style optimistic concurrency for aggregates with child entities
        // ValueGeneratedNever() tells EF Core this is NOT a store-generated value
        builder.Property(e => e.Version).IsConcurrencyToken().ValueGeneratedNever();

        builder.Ignore(e => e.DomainEvents);

        ConfigureAggregate(builder);
    }

    protected abstract void ConfigureAggregate(EntityTypeBuilder<TAggregate> builder);
}
