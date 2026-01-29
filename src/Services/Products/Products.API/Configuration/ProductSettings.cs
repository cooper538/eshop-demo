using System.ComponentModel.DataAnnotations;
using EShop.Common.Application.Configuration;

namespace Products.API.Configuration;

public class ProductSettings
{
    public const string SectionName = "Product";

    [Required]
    public ServiceInfo Service { get; init; } = new();

    [Required]
    public DatabaseSettings Database { get; init; } = new();

    public CacheSettings Cache { get; init; } = new();

    public StockReservationSettings StockReservation { get; init; } = new();
}

public class DatabaseSettings
{
    [Range(1, 300)]
    public int CommandTimeoutSec { get; init; } = 30;

    public bool EnableRetry { get; init; } = true;

    [Range(1, 10)]
    public int MaxRetryCount { get; init; } = 3;
}

public class CacheSettings
{
    public bool Enabled { get; init; } = true;

    [Range(1, 1440)]
    public int ExpirationMin { get; init; } = 60;
}

public class StockReservationSettings
{
    [Range(1, 1440)]
    public int DefaultDurationMinutes { get; init; } = 15;

    public StockExpirationSettings Expiration { get; init; } = new();

    public TimeSpan DefaultDuration => TimeSpan.FromMinutes(DefaultDurationMinutes);
}

public class StockExpirationSettings
{
    [Range(1, 60)]
    public int CheckIntervalMinutes { get; init; } = 1;

    [Range(10, 1000)]
    public int BatchSize { get; init; } = 100;

    public TimeSpan CheckInterval => TimeSpan.FromMinutes(CheckIntervalMinutes);
}
