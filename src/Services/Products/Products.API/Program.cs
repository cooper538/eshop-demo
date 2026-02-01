using EShop.Common.Api.Extensions;
using EShop.Common.Infrastructure.Configuration;
using EShop.Products.API;
using EShop.Products.Application;
using EShop.Products.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddYamlConfiguration("product");
builder.AddServiceDefaults();
builder.AddSerilog();

// Azure: Load secrets from Key Vault before other configuration
if (builder.Environment.IsProduction())
{
    builder.AddKeyVaultConfiguration();
}

builder.Services.AddHealthChecks().AddPostgresHealthCheck("productdb");

// TODO: Add authentication when needed - currently trusts Gateway
// User context can be read from X-User-Id, X-User-Email headers if propagated

builder.Services.AddApplication();
builder.AddInfrastructure();
builder.AddPresentation();

var app = builder.Build();

app.UseApiDefaults();
app.MapProductsEndpoints();
app.MapDefaultEndpoints();

app.Run();
