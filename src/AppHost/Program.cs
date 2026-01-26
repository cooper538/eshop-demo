var builder = DistributedApplication.CreateBuilder(args);

// INFRASTRUCTURE
var postgres = builder
    .AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin();

var productDb = postgres.AddDatabase("productdb");
var orderDb = postgres.AddDatabase("orderdb");
var notificationDb = postgres.AddDatabase("notificationdb");

var rabbitmq = builder
    .AddRabbitMQ("messaging")
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

// API GATEWAY (to be added in task-04)

// var gateway = builder
//     .AddProject<Projects.EShop_Gateway>("gateway")
//     .WithReference(productService)
//     .WithReference(orderService)
//     .WithExternalHttpEndpoints();

builder.Build().Run();
