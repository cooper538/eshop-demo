using System.ComponentModel.DataAnnotations;

namespace Gateway.API.Configuration;

public class GatewaySettings
{
    public const string SectionName = "Gateway";

    [Required]
    public ServiceInfo Service { get; init; } = new();
}

public class ServiceInfo
{
    [Required]
    [StringLength(50)]
    public string Name { get; init; } = string.Empty;

    [Required]
    public string Version { get; init; } = "1.0.0";
}
