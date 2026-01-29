using System.ComponentModel.DataAnnotations;
using EShop.Common.Application.Configuration;

namespace Order.API.Configuration;

public class OrderSettings
{
    public const string SectionName = "Order";

    [Required]
    public ServiceInfo Service { get; init; } = new();

    [Required]
    public DatabaseSettings Database { get; init; } = new();
}

public class DatabaseSettings
{
    [Range(1, 300)]
    public int CommandTimeoutSec { get; init; } = 30;

    public bool EnableRetry { get; init; } = true;

    [Range(1, 10)]
    public int MaxRetryCount { get; init; } = 3;
}
