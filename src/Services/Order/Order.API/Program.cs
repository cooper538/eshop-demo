using EShop.Common.Api.Extensions;
using EShop.Order.API;
using EShop.Order.Application;
using EShop.Order.Infrastructure;
using EShop.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddYamlConfiguration("order");
builder.AddServiceDefaults();
builder.AddSerilog();

// Azure: Load secrets from Key Vault before other configuration
if (builder.Environment.IsProduction())
{
    builder.AddKeyVaultConfiguration();
}

builder
    .Services.AddHealthChecks()
    .AddPostgresHealthCheck(ResourceNames.Databases.Order)
    .AddServiceHealthCheck(ResourceNames.Services.Product);

// TODO: Add authentication when needed - currently trusts Gateway
// User context can be read from X-User-Id, X-User-Email headers if propagated

builder.Services.AddApplication();
builder.AddInfrastructure();
builder.AddPresentation();

var app = builder.Build();

app.UseApiDefaults();
app.MapOrderEndpoints();
app.MapDefaultEndpoints();

app.Run();
