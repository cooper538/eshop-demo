using System.Globalization;
using System.Threading.RateLimiting;
using EShop.Common.Api.Extensions;
using EShop.Gateway.API.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EShop.Gateway.API;

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
                var retryAfterSeconds = 60;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    retryAfterSeconds = (int)retryAfter.TotalSeconds;
                    context.HttpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString(
                        CultureInfo.InvariantCulture
                    );
                }

                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Too Many Requests",
                    Detail =
                        $"Rate limit exceeded. Please retry after {retryAfterSeconds} seconds.",
                    Instance = context.HttpContext.Request.Path,
                };

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsJsonAsync(
                    problemDetails,
                    cancellationToken
                );
            };

            options.AddFixedWindowLimiter(
                RateLimitingSettings.FixedWindowPolicyName,
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
                "SwaggerCache",
                policy => policy.Expire(settings.OutputCache.SwaggerCacheDuration).Tag("swagger")
            );
        });

        return services;
    }
}
