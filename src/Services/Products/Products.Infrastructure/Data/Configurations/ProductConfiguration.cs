using EShop.Common.Infrastructure.Data;
using EShop.Products.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Products.Infrastructure.Data.Configurations;

public class ProductConfiguration : AggregateRootConfiguration<ProductEntity>
{
    protected override void ConfigureAggregate(EntityTypeBuilder<ProductEntity> builder)
    {
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();

        builder.Property(p => p.Description).HasMaxLength(2000);

        builder.Property(p => p.Price).HasPrecision(18, 2).IsRequired();

        builder.Property(p => p.Category).HasMaxLength(100);

        builder.Property(p => p.CreatedAt).IsRequired();

        builder.Property(p => p.UpdatedAt);
    }
}
