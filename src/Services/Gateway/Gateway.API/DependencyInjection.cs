using System.Globalization;
using System.Threading.RateLimiting;
using EShop.Common.Api.Extensions;
using EShop.Gateway.API.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Serilog;

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

        if (settings.Authentication.Enabled)
        {
            builder.Services.AddAuthentication(settings);
        }

        // HSTS for production - enforce HTTPS
        if (!builder.Environment.IsDevelopment())
        {
            builder.Services.AddHsts(options =>
            {
                options.MaxAge = TimeSpan.FromDays(365);
                options.IncludeSubDomains = true;
                options.Preload = true;
            });
        }

        return builder;
    }

    private static IServiceCollection AddAuthentication(
        this IServiceCollection services,
        GatewaySettings settings
    )
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(
                options =>
                {
                    options.Audience = settings.Authentication.AzureAd.Audience;

                    // Explicit token validation parameters for security
                    options.TokenValidationParameters.ValidateIssuer = true;
                    options.TokenValidationParameters.ValidateAudience = true;
                    options.TokenValidationParameters.ValidateLifetime = true;
                    options.TokenValidationParameters.ValidateIssuerSigningKey = true;
                    options.TokenValidationParameters.RequireSignedTokens = true;

                    // Prevent algorithm confusion attacks - whitelist only RS256
                    options.TokenValidationParameters.ValidAlgorithms = new[]
                    {
                        SecurityAlgorithms.RsaSha256,
                    };

                    // Security event logging
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Log.Warning(
                                "JWT authentication failed for {Path}: {Error}",
                                context.Request.Path,
                                context.Exception.Message
                            );
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Log.Information(
                                "JWT token validated for user {User} on {Path}",
                                context.Principal?.Identity?.Name ?? "unknown",
                                context.Request.Path
                            );
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            Log.Warning(
                                "JWT challenge issued for {Path}: {Error}",
                                context.Request.Path,
                                context.ErrorDescription ?? "No token provided"
                            );
                            return Task.CompletedTask;
                        },
                        OnForbidden = context =>
                        {
                            Log.Warning(
                                "JWT forbidden for user {User} on {Path}",
                                context.Principal?.Identity?.Name ?? "unknown",
                                context.Request.Path
                            );
                            return Task.CompletedTask;
                        },
                    };
                },
                options =>
                {
                    options.Instance = settings.Authentication.AzureAd.Instance;
                    options.TenantId = settings.Authentication.AzureAd.TenantId;
                    options.ClientId = settings.Authentication.AzureAd.ClientId;
                }
            );

        services.AddAuthorization();

        return services;
    }

    public static WebApplication MapGatewayEndpoints(
        this WebApplication app,
        GatewaySettings settings
    )
    {
        // HSTS must be first in pipeline for production
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseCorrelationId();

        if (settings.RateLimiting.Enabled)
        {
            app.UseRateLimiter();
        }

        if (settings.Authentication.Enabled)
        {
            app.UseAuthentication();
            app.UseAuthorization();
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
