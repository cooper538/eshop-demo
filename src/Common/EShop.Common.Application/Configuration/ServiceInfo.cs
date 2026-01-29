using System.ComponentModel.DataAnnotations;

namespace EShop.Common.Application.Configuration;

/// <summary>
/// Common service information settings shared across all services.
/// </summary>
public class ServiceInfo
{
    [Required]
    [StringLength(50)]
    public string Name { get; init; } = string.Empty;
}
