using EShop.ServiceDefaults;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin();

var productDb = postgres.AddDatabase(ResourceNames.Databases.Product);
var orderDb = postgres.AddDatabase(ResourceNames.Databases.Order);
var notificationDb = postgres.AddDatabase(ResourceNames.Databases.Notification);

var rabbitmq = builder
    .AddRabbitMQ(ResourceNames.Messaging)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithManagementPlugin();

// Migration service runs first and applies all database migrations
var migrationService = builder
    .AddProject<Projects.EShop_DatabaseMigration>("migration-service")
    .WithReference(productDb)
    .WithReference(orderDb)
    .WithReference(notificationDb)
    .WaitFor(productDb)
    .WaitFor(orderDb)
    .WaitFor(notificationDb);

// All services wait for migrations to complete
var productService = builder
    .AddProject<Projects.Products_API>("product-service")
    .WithHttpEndpoint()
    .WithHttpsEndpoint()
    .WithReference(productDb)
    .WithReference(rabbitmq)
    .WaitForCompletion(migrationService)
    .WaitFor(rabbitmq);

var orderService = builder
    .AddProject<Projects.Order_API>("order-service")
    .WithHttpEndpoint()
    .WithHttpsEndpoint()
    .WithReference(orderDb)
    .WithReference(rabbitmq)
    .WithReference(productService)
    .WaitForCompletion(migrationService)
    .WaitFor(rabbitmq);

var notificationService = builder
    .AddProject<Projects.EShop_NotificationService>("notification-service")
    .WithReference(notificationDb)
    .WithReference(rabbitmq)
    .WaitForCompletion(migrationService)
    .WaitFor(rabbitmq);

var analyticsService = builder
    .AddProject<Projects.EShop_AnalyticsService>("analytics-service")
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);

var gateway = builder
    .AddProject<Projects.Gateway_API>("gateway")
    .WithHttpEndpoint()
    .WithHttpsEndpoint()
    .WithReference(productService)
    .WithReference(orderService)
    .WaitFor(productService)
    .WaitFor(orderService)
    .WithExternalHttpEndpoints();

builder.Build().Run();
