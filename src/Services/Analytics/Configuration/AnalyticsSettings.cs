using System.ComponentModel.DataAnnotations;
using EShop.Common.Application.Configuration;

namespace EShop.AnalyticsService.Configuration;

public class AnalyticsSettings
{
    public const string SectionName = "Analytics";

    [Required]
    public ServiceInfo Service { get; init; } = new();
}
