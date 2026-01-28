using EShop.NotificationService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace EShop.NotificationService.Data;

/// <summary>
/// Database context for the Notification Service.
/// Tracks processed messages to ensure idempotent message handling (Inbox Pattern).
/// </summary>
public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options) { }

    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }
}
