namespace EShop.ServiceClients.Configuration;

public sealed class ResilienceOptions
{
    public RetryOptions Retry { get; set; } = new();
    public CircuitBreakerOptions CircuitBreaker { get; set; } = new();
}

public sealed class RetryOptions
{
    public int MaxRetryCount { get; set; } = 3;
    public int BaseDelayMs { get; set; } = 100;
}

public sealed class CircuitBreakerOptions
{
    public int FailuresBeforeBreaking { get; set; } = 5;
    public int BreakDurationSeconds { get; set; } = 30;
}
