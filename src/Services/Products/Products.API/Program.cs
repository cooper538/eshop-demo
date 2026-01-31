using EShop.Common.Api.Extensions;
using EShop.Products.API;
using EShop.Products.Application;
using EShop.Products.Infrastructure;
using EShop.ServiceDefaults;

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

builder.Services.AddApplication();
builder.AddInfrastructure();
builder.AddPresentation();

var app = builder.Build();

app.UseApiDefaults();
app.MapProductsEndpoints();
app.MapDefaultEndpoints();

app.Run();
