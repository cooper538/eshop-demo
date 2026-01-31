using EShop.Gateway.API;
using EShop.Gateway.API.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.AddYamlConfiguration("gateway");
builder.AddServiceDefaults();
builder.AddSerilog();

builder
    .Services.AddHealthChecks()
    .AddServiceHealthCheck("products-api")
    .AddServiceHealthCheck("order-api");

var settings =
    builder.Configuration.GetSection(GatewaySettings.SectionName).Get<GatewaySettings>()
    ?? new GatewaySettings();

builder.AddGatewayServices(settings);

var app = builder.Build();

app.MapGatewayEndpoints(settings);
app.MapDefaultEndpoints();

app.Run();
