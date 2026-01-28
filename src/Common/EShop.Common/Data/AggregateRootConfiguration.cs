using EShop.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Common.Data;

public abstract class AggregateRootConfiguration<TAggregate> : EntityConfiguration<TAggregate>
    where TAggregate : AggregateRoot
{
    protected override void ConfigureEntity(EntityTypeBuilder<TAggregate> builder)
    {
        builder.Property(e => e.Version).IsRowVersion();
        builder.Ignore(e => e.DomainEvents);

        ConfigureAggregate(builder);
    }

    protected abstract void ConfigureAggregate(EntityTypeBuilder<TAggregate> builder);
}
