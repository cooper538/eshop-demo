namespace EShop.ServiceClients.Configuration;

public sealed class ResilienceOptions
{
    public RetryOptions Retry { get; set; } = new();
}

public sealed class RetryOptions
{
    public int MaxRetryCount { get; set; } = 3;
    public int BaseDelayMs { get; set; } = 100;
    public int MaxBackoffMs { get; set; } = 5000;
    public double BackoffMultiplier { get; set; } = 2;
}
