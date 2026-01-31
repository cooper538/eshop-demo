using System.Reflection;
using EShop.NotificationService.Data;
using EShop.Order.Infrastructure.Data;
using EShop.Products.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.ArchitectureTests;

/// <summary>
/// Tests that verify EF Core model changes are captured in migrations.
/// These tests detect when a developer modifies entities but forgets to create a migration.
/// </summary>
[TestClass]
public class MigrationTests
{
    [TestMethod]
    public void ProductDbContext_ShouldNotHavePendingModelChanges()
    {
        AssertNoMissingMigrations<ProductDbContext>(CreateProductDbContext);
    }

    [TestMethod]
    public void OrderDbContext_ShouldNotHavePendingModelChanges()
    {
        AssertNoMissingMigrations<OrderDbContext>(CreateOrderDbContext);
    }

    [TestMethod]
    public void NotificationDbContext_ShouldNotHavePendingModelChanges()
    {
        AssertNoMissingMigrations<NotificationDbContext>(CreateNotificationDbContext);
    }

    private static void AssertNoMissingMigrations<TContext>(Func<TContext> contextFactory)
        where TContext : DbContext
    {
        // Create design-time services using the context's assembly (where migrations live)
        var contextAssembly = typeof(TContext).Assembly;
#pragma warning disable EF1001 // Internal EF Core API usage - required for migration diff check
        var builder = new DesignTimeServicesBuilder(
            contextAssembly,
            Assembly.GetExecutingAssembly(),
            new OperationReporter(new OperationReportHandler()),
            []
        );

        using var context = contextFactory();
        var provider = builder.Build(context);
        var dependencies = provider.GetRequiredService<MigrationsScaffolderDependencies>();

        // Get the snapshot model (what migrations have recorded)
        var modelSnapshot = dependencies.MigrationsAssembly.ModelSnapshot;
        var snapshotModel = dependencies.SnapshotModelProcessor.Process(modelSnapshot?.Model);
        var snapshotRelationalModel = snapshotModel?.GetRelationalModel();

        // Get the current model (what the code defines)
        var currentRelationalModel = dependencies.Model.GetRelationalModel();

        // Compare: if there are differences, a migration is missing
        var hasDifferences = dependencies.MigrationsModelDiffer.HasDifferences(
            snapshotRelationalModel,
            currentRelationalModel
        );
#pragma warning restore EF1001

        Assert.IsFalse(
            hasDifferences,
            $"{typeof(TContext).Name}: Model has changes not captured in migrations. "
                + "Run 'dotnet ef migrations add <Name>' to create a migration."
        );
    }

    private static ProductDbContext CreateProductDbContext()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseNpgsql("Host=localhost;Database=test")
            .Options;

        return new ProductDbContext(options);
    }

    private static OrderDbContext CreateOrderDbContext()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseNpgsql("Host=localhost;Database=test")
            .Options;

        return new OrderDbContext(options);
    }

    private static NotificationDbContext CreateNotificationDbContext()
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseNpgsql("Host=localhost;Database=test")
            .Options;

        return new NotificationDbContext(options);
    }
}
