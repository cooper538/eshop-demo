using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.Entities;

namespace Products.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();

        builder.Property(p => p.Description).HasMaxLength(2000);

        builder.Property(p => p.Price).HasPrecision(18, 2).IsRequired();

        builder.Property(p => p.StockQuantity).IsRequired();

        builder.Property(p => p.LowStockThreshold).IsRequired();

        builder.Property(p => p.Category).HasMaxLength(100);

        builder.Property(p => p.CreatedAt).IsRequired();

        builder.Property(p => p.UpdatedAt);

        // Optimistic concurrency
        builder.Property(p => p.Version).IsConcurrencyToken();
    }
}
