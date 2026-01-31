using EShop.Common.Infrastructure.Data;
using EShop.Products.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Products.Infrastructure.Data.Configurations;

public class StockConfiguration : AggregateRootConfiguration<StockEntity>
{
    protected override void ConfigureAggregate(EntityTypeBuilder<StockEntity> builder)
    {
        builder.Property(s => s.ProductId).IsRequired();

        builder.Property(s => s.Quantity).IsRequired();

        builder.Property(s => s.LowStockThreshold).IsRequired();

        builder.HasIndex(s => s.ProductId).IsUnique();

        builder
            .HasMany(s => s.Reservations)
            .WithOne()
            .HasForeignKey(r => r.StockId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
