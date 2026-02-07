using EShop.Order.Domain.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Order.Infrastructure.Data.Configurations;

public class ProductSnapshotConfiguration : IEntityTypeConfiguration<ProductSnapshot>
{
    public void Configure(EntityTypeBuilder<ProductSnapshot> builder)
    {
        builder.ToTable("ProductSnapshots");
        builder.HasKey(p => p.ProductId);
        builder.Property(p => p.ProductId).ValueGeneratedNever();
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(p => p.LastUpdated).IsRequired();
    }
}
