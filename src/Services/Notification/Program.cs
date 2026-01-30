using EShop.NotificationService;

var builder = Host.CreateApplicationBuilder(args);

builder.AddYamlConfiguration("notification");
builder.AddServiceDefaults();
builder.AddSerilog();

builder.Services.AddHealthChecks().AddPostgresHealthCheck("notificationdb");

builder.AddNotificationServices();

var host = builder.Build();
host.Run();
