using EShop.NotificationService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.NotificationService.Data.Configuration;

public class ProcessedMessageConfiguration : IEntityTypeConfiguration<ProcessedMessage>
{
    public void Configure(EntityTypeBuilder<ProcessedMessage> builder)
    {
        builder.ToTable("ProcessedMessages");

        // Composite primary key for idempotency
        builder.HasKey(x => new { x.MessageId, x.ConsumerType });

        builder.Property(x => x.MessageId).IsRequired();

        builder.Property(x => x.ConsumerType).IsRequired().HasMaxLength(255);

        builder.Property(x => x.ProcessedAt).IsRequired();

        // Index for cleanup queries (finding old records to delete)
        builder.HasIndex(x => x.ProcessedAt);
    }
}
