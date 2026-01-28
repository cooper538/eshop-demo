namespace EShop.ServiceClients.Configuration;

public sealed class ServiceClientOptions
{
    public const string SectionName = "ServiceClients";

    /// <summary>
    /// Default timeout for service calls in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Resilience policies configuration (retry, circuit breaker).
    /// </summary>
    public ResilienceOptions Resilience { get; set; } = new();

    /// <summary>
    /// Product Service endpoint configuration.
    /// </summary>
    public ServiceEndpoints ProductService { get; set; } = new();
}
