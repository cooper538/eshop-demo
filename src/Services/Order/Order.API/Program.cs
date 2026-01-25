using EShop.Common.Extensions;
using FluentValidation;
using Order.Application.Data;
using Order.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Aspire ServiceDefaults
builder.AddServiceDefaults();

// API
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// MediatR + Behaviors
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IOrderDbContext>());
builder.Services.AddCommonBehaviors();

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
