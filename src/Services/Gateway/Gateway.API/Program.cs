using EShop.Common.Extensions;
using Gateway.API.Configuration;
using NetEscapades.Configuration.Yaml;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment.EnvironmentName;

// YAML Configuration
builder
    .Configuration.AddYamlFile("gateway.settings.yaml", optional: false, reloadOnChange: true)
    .AddYamlFile($"gateway.settings.{env}.yaml", optional: true, reloadOnChange: true);

// Bind and validate settings (fail-fast on invalid config)
builder
    .Services.AddOptions<GatewaySettings>()
    .BindConfiguration(GatewaySettings.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Aspire ServiceDefaults
builder.AddServiceDefaults();

// YARP Reverse Proxy with Service Discovery
builder
    .Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

// CorrelationId support
builder.Services.AddCorrelationId();

var app = builder.Build();

// Middleware pipeline
app.UseCorrelationId();

app.MapReverseProxy();
app.MapDefaultEndpoints();

app.Run();
