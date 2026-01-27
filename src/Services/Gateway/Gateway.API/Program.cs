using System.Globalization;
using System.Threading.RateLimiting;
using EShop.Common.Extensions;
using Gateway.API.Configuration;
using Microsoft.AspNetCore.RateLimiting;
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

// Rate Limiting
var rateLimitSettings =
    builder
        .Configuration.GetSection($"{GatewaySettings.SectionName}:RateLimiting")
        .Get<RateLimitingSettings>()
    ?? new RateLimitingSettings();

if (rateLimitSettings.Enabled)
{
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.OnRejected = async (context, cancellationToken) =>
        {
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter = (
                    (int)retryAfter.TotalSeconds
                ).ToString(CultureInfo.InvariantCulture);
            }

            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.WriteAsync(
                """{"error":"Rate limit exceeded. Please try again later."}""",
                cancellationToken
            );
        };

        options.AddFixedWindowLimiter(
            "fixed",
            opt =>
            {
                opt.PermitLimit = rateLimitSettings.PermitLimit;
                opt.Window = TimeSpan.FromSeconds(rateLimitSettings.WindowSeconds);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = rateLimitSettings.QueueLimit;
            }
        );
    });
}

// Output Caching for Products
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy(
        "ProductsCache",
        policy => policy.Expire(TimeSpan.FromMinutes(5)).Tag("products")
    );

    options.AddPolicy(
        "ProductDetailCache",
        policy =>
            policy.Expire(TimeSpan.FromMinutes(2)).SetVaryByRouteValue("catch-all").Tag("products")
    );
});

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

if (rateLimitSettings.Enabled)
{
    app.UseRateLimiter();
}

app.UseOutputCache();
app.MapReverseProxy();
app.MapDefaultEndpoints();

app.Run();
