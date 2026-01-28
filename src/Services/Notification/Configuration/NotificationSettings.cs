using System.ComponentModel.DataAnnotations;
using EShop.Common.Configuration;

namespace EShop.NotificationService.Configuration;

public class NotificationSettings
{
    public const string SectionName = "Notification";

    [Required]
    public ServiceInfo Service { get; init; } = new();

    [Required]
    public EmailSettings Email { get; init; } = new();
}

public class EmailSettings
{
    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string AdminEmail { get; init; } = "admin@eshop.local";
}
