using EShop.Common.Extensions;
using EShop.ServiceClients.Extensions;
using FluentValidation;
using NetEscapades.Configuration.Yaml;
using Order.API.Configuration;
using Order.Application.Data;
using Order.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment.EnvironmentName;

// YAML Configuration
builder
    .Configuration.AddYamlFile("order.settings.yaml", optional: false, reloadOnChange: true)
    .AddYamlFile($"order.settings.{env}.yaml", optional: true, reloadOnChange: true);

// Bind and validate settings (fail-fast on invalid config)
builder
    .Services.AddOptions<OrderSettings>()
    .BindConfiguration(OrderSettings.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Aspire ServiceDefaults
builder.AddServiceDefaults();

// Service Clients (gRPC)
builder.Services.AddServiceClients(builder.Configuration, builder.Environment);

// API
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// MediatR + Behaviors
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IOrderDbContext>());
builder.Services.AddCommonBehaviors();
builder.Services.AddDomainEvents();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<IOrderDbContext>();

// Infrastructure (DbContext)
builder.AddInfrastructure();

// Error handling
builder.Services.AddErrorHandling();
builder.Services.AddCorrelationId();

var app = builder.Build();

// Middleware pipeline
app.UseCorrelationId();
app.UseErrorHandling();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Orders API"));
}

app.MapControllers();
app.MapDefaultEndpoints();

app.Run();
