using EShop.Common.Data;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Entities;

namespace Order.Infrastructure.Data.Configurations;

public class OrderConfiguration : AggregateRootConfiguration<OrderEntity>
{
    protected override void ConfigureAggregate(EntityTypeBuilder<OrderEntity> builder)
    {
        builder.Property(o => o.CustomerId).IsRequired();

        builder.Property(o => o.CustomerEmail).HasMaxLength(320).IsRequired();

        builder.Property(o => o.Status).IsRequired();

        builder.Property(o => o.TotalAmount).HasPrecision(18, 2).IsRequired();

        builder.Property(o => o.RejectionReason).HasMaxLength(500);

        builder.Property(o => o.CreatedAt).IsRequired();

        builder.Property(o => o.UpdatedAt);

        builder.OwnsMany(
            o => o.Items,
            itemBuilder =>
            {
                itemBuilder.Property(i => i.ProductId).IsRequired();
                itemBuilder.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
                itemBuilder.Property(i => i.Quantity).IsRequired();
                itemBuilder.Property(i => i.UnitPrice).HasPrecision(18, 2).IsRequired();
            }
        );
    }
}
