using System.Globalization;
using System.Threading.RateLimiting;
using EShop.Common.Api.Extensions;
using Gateway.API.Configuration;
using Microsoft.AspNetCore.RateLimiting;

namespace Gateway.API;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddGatewayServices(
        this IHostApplicationBuilder builder,
        GatewaySettings settings
    )
    {
        builder
            .Services.AddOptions<GatewaySettings>()
            .BindConfiguration(GatewaySettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        if (settings.RateLimiting.Enabled)
        {
            builder.Services.AddRateLimiting(settings);
        }

        builder.Services.AddOutputCaching(settings);

        builder
            .Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
            .AddServiceDiscoveryDestinationResolver();

        builder.Services.AddCorrelationId();

        return builder;
    }

    public static WebApplication MapGatewayEndpoints(
        this WebApplication app,
        GatewaySettings settings
    )
    {
        app.UseCorrelationId();

        if (settings.RateLimiting.Enabled)
        {
            app.UseRateLimiter();
        }

        app.UseOutputCache();
        app.MapReverseProxy();

        return app;
    }

    private static IServiceCollection AddRateLimiting(
        this IServiceCollection services,
        GatewaySettings settings
    )
    {
        services.AddRateLimiter(options =>
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
                    opt.PermitLimit = settings.RateLimiting.PermitLimit;
                    opt.Window = TimeSpan.FromSeconds(settings.RateLimiting.WindowSeconds);
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = settings.RateLimiting.QueueLimit;
                }
            );
        });

        return services;
    }

    private static IServiceCollection AddOutputCaching(
        this IServiceCollection services,
        GatewaySettings settings
    )
    {
        services.AddOutputCache(options =>
        {
            options.AddPolicy(
                "ProductsCache",
                policy =>
                    policy.Expire(settings.OutputCache.ProductsListCacheDuration).Tag("products")
            );

            options.AddPolicy(
                "ProductDetailCache",
                policy =>
                    policy
                        .Expire(settings.OutputCache.ProductDetailCacheDuration)
                        .SetVaryByRouteValue("catch-all")
                        .Tag("products")
            );
        });

        return services;
    }
}
