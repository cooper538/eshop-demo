using EShop.Common.Api.Extensions;
using Products.API;
using Products.Application;
using Products.Infrastructure;

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
