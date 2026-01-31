using EShop.Common.Api.Extensions;
using EShop.Order.API;
using EShop.Order.Application;
using EShop.Order.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddYamlConfiguration("order");
builder.AddServiceDefaults();
builder.AddSerilog();

builder
    .Services.AddHealthChecks()
    .AddPostgresHealthCheck("orderdb")
    .AddServiceHealthCheck("products-api");

builder.Services.AddApplication();
builder.AddInfrastructure();
builder.AddPresentation();

var app = builder.Build();

app.UseApiDefaults();
app.MapOrderEndpoints();
app.MapDefaultEndpoints();

app.Run();
