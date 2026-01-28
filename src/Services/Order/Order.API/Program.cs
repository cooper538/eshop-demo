using EShop.Common.Correlation.MassTransit;
using EShop.Common.Extensions;
using EShop.ServiceClients.Extensions;
using EShop.ServiceDefaults;
using FluentValidation;
using MassTransit;
using Order.API.Configuration;
using Order.Application.Data;
using Order.Infrastructure;
using Order.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment.EnvironmentName;

builder
    .Configuration.AddYamlFile("order.settings.yaml", optional: false, reloadOnChange: true)
    .AddYamlFile($"order.settings.{env}.yaml", optional: true, reloadOnChange: true);

builder
    .Services.AddOptions<OrderSettings>()
    .BindConfiguration(OrderSettings.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.AddServiceDefaults();
builder.AddSerilog();

builder.Services.AddServiceClients(builder.Configuration, builder.Environment);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IOrderDbContext>());
builder.Services.AddCommonBehaviors();
builder.Services.AddDomainEvents();
builder.Services.AddDateTimeProvider();

builder.Services.AddValidatorsFromAssemblyContaining<IOrderDbContext>();

builder.AddInfrastructure();

builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<OrderDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.UsingRabbitMq(
        (context, cfg) =>
        {
            var connectionString = builder.Configuration.GetConnectionString(
                ResourceNames.Messaging
            );
            if (!string.IsNullOrEmpty(connectionString))
            {
                cfg.Host(new Uri(connectionString));
            }

            cfg.UseCorrelationIdFilters(context);
            cfg.ConfigureEndpoints(context);
        }
    );
});

builder.Services.AddErrorHandling();
builder.Services.AddCorrelationId();

var app = builder.Build();

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
