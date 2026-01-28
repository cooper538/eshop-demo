using System.Globalization;
using System.Threading.RateLimiting;
using EShop.Common.Extensions;
using Gateway.API.Configuration;
using Microsoft.AspNetCore.RateLimiting;

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

// Read settings for startup configuration
var gatewaySettings =
    builder.Configuration.GetSection(GatewaySettings.SectionName).Get<GatewaySettings>()
    ?? new GatewaySettings();

// Aspire ServiceDefaults
builder.AddServiceDefaults();
builder.AddSerilog();

// Rate Limiting
if (gatewaySettings.RateLimiting.Enabled)
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
                opt.PermitLimit = gatewaySettings.RateLimiting.PermitLimit;
                opt.Window = TimeSpan.FromSeconds(gatewaySettings.RateLimiting.WindowSeconds);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = gatewaySettings.RateLimiting.QueueLimit;
            }
        );
    });
}

// Output Caching for Products
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy(
        "ProductsCache",
        policy =>
            policy.Expire(gatewaySettings.OutputCache.ProductsListCacheDuration).Tag("products")
    );

    options.AddPolicy(
        "ProductDetailCache",
        policy =>
            policy
                .Expire(gatewaySettings.OutputCache.ProductDetailCacheDuration)
                .SetVaryByRouteValue("catch-all")
                .Tag("products")
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

if (gatewaySettings.RateLimiting.Enabled)
{
    app.UseRateLimiter();
}

app.UseOutputCache();
app.MapReverseProxy();
app.MapDefaultEndpoints();

app.Run();
