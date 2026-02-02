using System.ComponentModel.DataAnnotations;
using EShop.Common.Application.Configuration;

namespace EShop.Gateway.API.Configuration;

public class GatewaySettings
{
    public const string SectionName = "Gateway";

    [Required]
    public ServiceInfo Service { get; init; } = new();

    public RateLimitingSettings RateLimiting { get; init; } = new();

    public OutputCacheSettings OutputCache { get; init; } = new();

    public AuthenticationSettings Authentication { get; init; } = new();
}

public class AuthenticationSettings
{
    public bool Enabled { get; init; }

    public bool UseTestScheme { get; init; }

    public string TestSecretKey { get; init; } = string.Empty;

    public AzureAdSettings AzureAd { get; init; } = new();
}

public class AzureAdSettings
{
    public string Instance { get; init; } = "https://login.microsoftonline.com/";

    public string TenantId { get; init; } = string.Empty;

    public string ClientId { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;
}

public class RateLimitingSettings
{
    public const string FixedWindowPolicyName = "fixed";

    public bool Enabled { get; init; } = true;

    [Range(1, 10000)]
    public int PermitLimit { get; init; } = 100;

    [Range(1, 3600)]
    public int WindowSeconds { get; init; } = 60;

    [Range(0, 100)]
    public int QueueLimit { get; init; } = 0;
}

public class OutputCacheSettings
{
    [Range(1, 1440)]
    public int ProductsListCacheMinutes { get; init; } = 5;

    [Range(1, 1440)]
    public int SwaggerCacheMinutes { get; init; } = 1440;

    public TimeSpan ProductsListCacheDuration => TimeSpan.FromMinutes(ProductsListCacheMinutes);
    public TimeSpan SwaggerCacheDuration => TimeSpan.FromMinutes(SwaggerCacheMinutes);
}
