using EShop.Common.Infrastructure.Configuration;
using EShop.NotificationService;

var builder = Host.CreateApplicationBuilder(args);

builder.AddYamlConfiguration("notification");
builder.AddServiceDefaults();
builder.AddSerilog();

// Azure: Load secrets from Key Vault before other configuration
if (builder.Environment.IsProduction())
{
    builder.AddKeyVaultConfiguration();
}

builder.Services.AddHealthChecks().AddPostgresHealthCheck("notificationdb");

// TODO: Add authentication when needed - currently trusts Gateway
// User context can be read from message headers if propagated

builder.AddNotificationServices();

var host = builder.Build();
host.Run();
