namespace EShop.ServiceClients.Configuration;

public sealed class ServiceClientOptions
{
    public const string SectionName = "ServiceClients";

    public int TimeoutSeconds { get; set; } = 30;

    public ResilienceOptions Resilience { get; set; } = new();

    public ServiceEndpoints ProductService { get; set; } = new();
}
