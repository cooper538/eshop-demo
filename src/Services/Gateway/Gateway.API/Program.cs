using EShop.Gateway.API;
using EShop.Gateway.API.Configuration;
using EShop.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddYamlConfiguration("gateway");
builder.AddServiceDefaults();
builder.AddSerilog();

// Azure: Load secrets from Key Vault before other configuration
if (builder.Environment.IsProduction())
{
    builder.AddKeyVaultConfiguration();
}

builder
    .Services.AddHealthChecks()
    .AddServiceHealthCheck(ResourceNames.Services.Product)
    .AddServiceHealthCheck(ResourceNames.Services.Order);

var settings =
    builder.Configuration.GetSection(GatewaySettings.SectionName).Get<GatewaySettings>()
    ?? new GatewaySettings();

builder.AddGatewayServices(settings);

var app = builder.Build();

app.MapGatewayEndpoints(settings);
app.MapDefaultEndpoints();

app.Run();
