using EShop.Common.Api.Extensions;
using Order.API;
using Order.Application;
using Order.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddYamlConfiguration("order");
builder.AddServiceDefaults();
builder.AddSerilog();

builder.Services.AddApplication();
builder.AddInfrastructure();
builder.AddPresentation();

var app = builder.Build();

app.UseApiDefaults();
app.MapOrderEndpoints();
app.MapDefaultEndpoints();

app.Run();
