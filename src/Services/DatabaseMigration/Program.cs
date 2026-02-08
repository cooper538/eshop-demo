using EShop.Common.Infrastructure.Configuration;
using EShop.DatabaseMigration;
using EShop.NotificationService.Data;
using EShop.Order.Infrastructure.Data;
using EShop.Products.Infrastructure.Data;
using EShop.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddSerilog();
builder.AddServiceDefaults();

if (builder.Environment.IsProduction())
{
    builder.AddKeyVaultConfiguration();
}

builder
    .Services.AddHealthChecks()
    .AddPostgresHealthCheck(ResourceNames.Databases.Order)
    .AddPostgresHealthCheck(ResourceNames.Databases.Product)
    .AddPostgresHealthCheck(ResourceNames.Databases.Notification);

builder.Services.AddSingleton<MigrationTracker>();
builder.AddMigratableDatabase<OrderDbContext>(ResourceNames.Databases.Order);
builder.AddMigratableDatabase<ProductDbContext>(ResourceNames.Databases.Product);
builder.AddProductSeeding();
builder.AddMigratableDatabase<NotificationDbContext>(ResourceNames.Databases.Notification);
builder.Services.AddHostedService<MigrationCompletionService>();

var host = builder.Build();
host.Run();
