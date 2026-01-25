using EShop.Common.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.Entities;

namespace Products.Infrastructure.Data.Configurations;

public class StockReservationConfiguration : EntityConfiguration<StockReservationEntity>
{
    protected override void ConfigureEntity(EntityTypeBuilder<StockReservationEntity> builder)
    {
        builder.Property(e => e.OrderId).IsRequired();

        builder.Property(e => e.ProductId).IsRequired();

        builder.Property(e => e.Quantity).IsRequired();

        builder.Property(e => e.ReservedAt).IsRequired();

        builder.Property(e => e.ExpiresAt).IsRequired();

        builder.Property(e => e.ReleasedAt);

        builder.Property(e => e.Status).IsRequired();

        builder.HasIndex(e => e.OrderId);

        builder.HasIndex(e => e.ProductId);

        builder.HasIndex(e => new { e.Status, e.ExpiresAt });
    }
}
