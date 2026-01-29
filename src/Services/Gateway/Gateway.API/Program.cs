using Gateway.API;
using Gateway.API.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.AddYamlConfiguration("gateway");
builder.AddServiceDefaults();
builder.AddSerilog();

var settings =
    builder.Configuration.GetSection(GatewaySettings.SectionName).Get<GatewaySettings>()
    ?? new GatewaySettings();

builder.AddGatewayServices(settings);

var app = builder.Build();

app.MapGatewayEndpoints(settings);
app.MapDefaultEndpoints();

app.Run();
