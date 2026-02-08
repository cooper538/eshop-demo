using System.ComponentModel.DataAnnotations;

namespace EShop.Products.Infrastructure.Data;

public class SeedingSettings
{
    [Range(100, 100_000)]
    public int ProductCount { get; init; } = 1_000;

    [Range(100, 5_000)]
    public int BatchSize { get; init; } = 500;
}
