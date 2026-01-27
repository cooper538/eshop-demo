using System.ComponentModel.DataAnnotations;

namespace Gateway.API.Configuration;

public class GatewaySettings
{
    public const string SectionName = "Gateway";

    [Required]
    public ServiceInfo Service { get; init; } = new();

    public RateLimitingSettings RateLimiting { get; init; } = new();
}

public class ServiceInfo
{
    [Required]
    [StringLength(50)]
    public string Name { get; init; } = string.Empty;

    [Required]
    public string Version { get; init; } = "1.0.0";
}

public class RateLimitingSettings
{
    public bool Enabled { get; init; } = true;

    [Range(1, 10000)]
    public int PermitLimit { get; init; } = 100;

    [Range(1, 3600)]
    public int WindowSeconds { get; init; } = 60;

    [Range(0, 100)]
    public int QueueLimit { get; init; } = 0;
}
