using EShop.ServiceDefaults;

var builder = DistributedApplication.CreateBuilder(args);

// INFRASTRUCTURE
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

// SERVICES
var productService = builder
    .AddProject<Projects.Products_API>("product-service")
    .WithReference(productDb)
    .WaitFor(productDb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);

var orderService = builder
    .AddProject<Projects.Order_API>("order-service")
    .WithReference(orderDb)
    .WaitFor(orderDb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(productService);

var notificationService = builder
    .AddProject<Projects.EShop_NotificationService>("notification-service")
    .WithReference(notificationDb)
    .WaitFor(notificationDb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);

// API GATEWAY
var gateway = builder
    .AddProject<Projects.Gateway_API>("gateway")
    .WithReference(productService)
    .WithReference(orderService)
    .WithExternalHttpEndpoints();

builder.Build().Run();
