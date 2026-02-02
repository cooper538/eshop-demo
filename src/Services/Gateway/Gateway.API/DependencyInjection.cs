using System.Globalization;
using System.Text;
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
        if (settings.Authentication.UseTestScheme)
        {
            services.AddTestAuthentication(settings);
        }
        else
        {
            services.AddAzureAdAuthentication(settings);
        }

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = options.DefaultPolicy;
        });

        return services;
    }

    private static void AddTestAuthentication(
        this IServiceCollection services,
        GatewaySettings settings
    )
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "https://test-issuer.local",
                    ValidateAudience = true,
                    ValidAudience = "api://eshop-api",
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(settings.Authentication.TestSecretKey)
                    ),
                    ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                    ClockSkew = TimeSpan.Zero,
                };

                options.Events = CreateJwtBearerEvents();
            });
    }

    private static void AddAzureAdAuthentication(
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

                    options.TokenValidationParameters.ValidateIssuer = true;
                    options.TokenValidationParameters.ValidateAudience = true;
                    options.TokenValidationParameters.ValidateLifetime = true;
                    options.TokenValidationParameters.ValidateIssuerSigningKey = true;
                    options.TokenValidationParameters.RequireSignedTokens = true;

                    options.TokenValidationParameters.ValidAlgorithms = new[]
                    {
                        SecurityAlgorithms.RsaSha256,
                    };

                    options.Events = CreateJwtBearerEvents();
                },
                options =>
                {
                    options.Instance = settings.Authentication.AzureAd.Instance;
                    options.TenantId = settings.Authentication.AzureAd.TenantId;
                    options.ClientId = settings.Authentication.AzureAd.ClientId;
                }
            );
    }

    private static JwtBearerEvents CreateJwtBearerEvents()
    {
        return new JwtBearerEvents
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
                    context.Request.Path,
                    context.Principal?.Identity?.Name ?? "unknown"
                );
                return Task.CompletedTask;
            },
        };
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
