using EShop.Common.Extensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Products.Application.Data;
using Products.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Aspire ServiceDefaults
builder.AddServiceDefaults();

// API
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// MediatR + Behaviors
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IProductDbContext>());
builder.Services.AddCommonBehaviors();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<IProductDbContext>();

// EF Core
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("productdb"))
);
builder.Services.AddScoped<IProductDbContext>(sp => sp.GetRequiredService<ProductDbContext>());

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
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Products API"));
}

app.MapControllers();
app.MapDefaultEndpoints();

app.Run();
