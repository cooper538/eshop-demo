using EShop.DatabaseMigration;
using EShop.NotificationService.Data;
using EShop.Order.Infrastructure.Data;
using EShop.Products.Infrastructure.Data;
using EShop.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddSerilog();
builder.AddServiceDefaults();

builder
    .Services.AddHealthChecks()
    .AddPostgresHealthCheck(ResourceNames.Databases.Order)
    .AddPostgresHealthCheck(ResourceNames.Databases.Product)
    .AddPostgresHealthCheck(ResourceNames.Databases.Notification);

builder.AddNpgsqlDbContext<OrderDbContext>(ResourceNames.Databases.Order);
builder.AddNpgsqlDbContext<ProductDbContext>(ResourceNames.Databases.Product);
builder.AddNpgsqlDbContext<NotificationDbContext>(ResourceNames.Databases.Notification);

builder.Services.AddSingleton<MigrationTracker>();
builder.Services.AddHostedService<DbInitializer<OrderDbContext>>();
builder.Services.AddHostedService<DbInitializer<ProductDbContext>>();
builder.Services.AddHostedService<DbInitializer<NotificationDbContext>>();
builder.Services.AddHostedService<MigrationCompletionService>();

var host = builder.Build();
host.Run();
