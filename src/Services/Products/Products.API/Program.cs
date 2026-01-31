using EShop.Common.Api.Extensions;
using EShop.Products.API;
using EShop.Products.Application;
using EShop.Products.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddYamlConfiguration("product");
builder.AddServiceDefaults();
builder.AddSerilog();

builder.Services.AddHealthChecks().AddPostgresHealthCheck("productdb");

builder.Services.AddApplication();
builder.AddInfrastructure();
builder.AddPresentation();

var app = builder.Build();

app.UseApiDefaults();
app.MapProductsEndpoints();
app.MapDefaultEndpoints();

app.Run();
