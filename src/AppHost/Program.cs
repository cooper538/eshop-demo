using EShop.AppHost;
using EShop.ServiceDefaults;

var builder = DistributedApplication.CreateBuilder(args);

var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var postgres = builder
    .AddPostgres("postgres", password: postgresPassword)
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
    .AddProject<Projects.EShop_DatabaseMigration>(ResourceNames.Services.Migration)
    .WithReference(productDb)
    .WithReference(orderDb)
    .WithReference(notificationDb)
    .WaitFor(productDb)
    .WaitFor(orderDb)
    .WaitFor(notificationDb);

// All services wait for migrations to complete
var productService = builder
    .AddProject<Projects.EShop_Products_API>(ResourceNames.Services.Product)
    .WithHttpEndpoint()
    .WithHttpsEndpoint()
    .WithReference(productDb)
    .WithReference(rabbitmq)
    .WaitForCompletion(migrationService)
    .WaitFor(rabbitmq);

var orderService = builder
    .AddProject<Projects.EShop_Order_API>(ResourceNames.Services.Order)
    .WithHttpEndpoint()
    .WithHttpsEndpoint()
    .WithReference(orderDb)
    .WithReference(rabbitmq)
    .WithReference(productService)
    .WaitForCompletion(migrationService)
    .WaitFor(rabbitmq);

var notificationService = builder
    .AddProject<Projects.EShop_NotificationService>(ResourceNames.Services.Notification)
    .WithReference(notificationDb)
    .WithReference(rabbitmq)
    .WaitForCompletion(migrationService)
    .WaitFor(rabbitmq);

var analyticsService = builder
    .AddProject<Projects.EShop_AnalyticsService>(ResourceNames.Services.Analytics)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);

var gateway = builder
    .AddProject<Projects.EShop_Gateway_API>(ResourceNames.Services.Gateway)
    .WithHttpEndpoint(port: 64887)
    .WithHttpsEndpoint()
    .WithReference(productService)
    .WithReference(orderService)
    .WaitFor(productService)
    .WaitFor(orderService)
    .WithExternalHttpEndpoints();

builder.ConfigureDockerComposePublishing(
    new AppResources(
        postgres,
        rabbitmq,
        migrationService,
        productService,
        orderService,
        notificationService,
        analyticsService,
        gateway
    )
);

builder.Build().Run();
