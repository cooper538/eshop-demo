using EShop.Common.Correlation.MassTransit;
using EShop.Common.Data;
using EShop.Common.Extensions;
using EShop.Common.Grpc;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NetEscapades.Configuration.Yaml;
using Products.API.Configuration;
using Products.API.Grpc;
using Products.Application.Data;
using Products.Infrastructure.BackgroundJobs;
using Products.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment.EnvironmentName;

// YAML Configuration
builder
    .Configuration.AddYamlFile("product.settings.yaml", optional: false, reloadOnChange: true)
    .AddYamlFile($"product.settings.{env}.yaml", optional: true, reloadOnChange: true);

// Bind and validate settings (fail-fast on invalid config)
builder
    .Services.AddOptions<ProductSettings>()
    .BindConfiguration(ProductSettings.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Aspire ServiceDefaults
builder.AddServiceDefaults();

// API
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// gRPC
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<CorrelationIdServerInterceptor>();
    options.Interceptors.Add<GrpcLoggingInterceptor>();
    options.Interceptors.Add<GrpcValidationInterceptor>();
    options.Interceptors.Add<GrpcExceptionInterceptor>();
});

// gRPC request validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// MediatR + Behaviors
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IProductDbContext>());
builder.Services.AddCommonBehaviors();
builder.Services.AddDomainEvents();
builder.Services.AddDateTimeProvider();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<IProductDbContext>();

// EF Core
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("productdb"))
);
builder.Services.AddScoped<IProductDbContext>(sp => sp.GetRequiredService<ProductDbContext>());
builder.Services.AddScoped<IChangeTrackerAccessor>(sp => sp.GetRequiredService<ProductDbContext>());
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ProductDbContext>());

// MassTransit with RabbitMQ and Entity Framework Outbox
builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<ProductDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.UsingRabbitMq(
        (context, cfg) =>
        {
            var connectionString = builder.Configuration.GetConnectionString("messaging");
            if (!string.IsNullOrEmpty(connectionString))
            {
                cfg.Host(new Uri(connectionString));
            }

            cfg.UseCorrelationIdFilters(context);
            cfg.ConfigureEndpoints(context);
        }
    );
});

// Error handling
builder.Services.AddErrorHandling();
builder.Services.AddCorrelationId();

// Background jobs
builder.Services.AddHostedService<StockReservationExpirationJob>();

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
app.MapGrpcService<ProductGrpcService>();
app.MapDefaultEndpoints();

app.Run();
