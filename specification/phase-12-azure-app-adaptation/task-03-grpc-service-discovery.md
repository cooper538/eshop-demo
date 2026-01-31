# Task 03: gRPC Service Discovery

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | in_progress |
| Dependencies | - |

## Summary
Configure gRPC clients for Azure Container Apps service discovery using FQDN pattern instead of Aspire service references.

## Scope
- [ ] Create `GrpcServiceDiscoveryExtensions.cs` in `EShop.Common.Infrastructure/Grpc/`
- [ ] Implement Azure Container Apps service URL resolution
- [ ] Configure gRPC clients to use HTTP/2 over HTTPS for Container Apps internal communication
- [ ] Add `AddGrpcClientAzure<T>()` extension method
- [ ] Handle both local (Aspire) and Azure (Container Apps) environments
- [ ] Configure channel options for Azure networking

## Implementation Notes

Container Apps uses internal FQDN pattern for service discovery:
`{service-name}.internal.{environment-name}.{region}.azurecontainerapps.io`

```csharp
// Update RegisterGrpcClients in EShop.ServiceClients/Extensions/ServiceCollectionExtensions.cs
// to conditionally skip Aspire service discovery in Production (Azure)

private static void RegisterGrpcClients(
    IServiceCollection services,
    ServiceClientOptions options,
    IHostEnvironment environment)
{
    var retryOptions = options.Resilience.Retry;
    var serviceConfig = CreateServiceConfig(retryOptions);

    services.AddTransient<LoggingInterceptor>();
    services.AddTransient<CorrelationIdClientInterceptor>();

    var grpcClientBuilder = services
        .AddGrpcClient<ProductService.ProductServiceClient>(o =>
        {
            o.Address = new Uri(options.ProductService.Url);
        })
        .ConfigureChannel(o =>
        {
            o.ServiceConfig = serviceConfig;
        });

    // Only use Aspire service discovery in Development
    // In Production (Azure), URLs are configured via appsettings.Production.yaml
    if (environment.IsDevelopment())
    {
        grpcClientBuilder.AddServiceDiscovery();
        grpcClientBuilder.ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            return handler;
        });
    }
    else
    {
        // Production (Azure) - use SocketsHttpHandler for HTTP/2 multiplexing
        grpcClientBuilder.ConfigurePrimaryHttpMessageHandler(() =>
            new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true
            });
    }

    grpcClientBuilder
        .AddInterceptor<CorrelationIdClientInterceptor>()
        .AddInterceptor<LoggingInterceptor>();

    services.AddScoped<IProductServiceClient, GrpcProductServiceClient>();
}
```

## Service URL Configuration (Production/Azure)

```yaml
# order.settings.Production.yaml
ServiceClients:
  ProductService:
    Url: https://product-service.internal.eshop-env.westeurope.azurecontainerapps.io
```

## Files to Create/Modify

| Action | File |
|--------|------|
| MODIFY | `src/Common/EShop.ServiceClients/Extensions/ServiceCollectionExtensions.cs` |
| CREATE | `src/Services/Order/Order.API/order.settings.Production.yaml` |

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 8.2 Service Discovery)
- -> [internal-api-communication.md](../high-level-specs/internal-api-communication.md) (gRPC configuration)

---
## Notes
- Container Apps internal communication uses HTTPS by default
- gRPC requires HTTP/2 which is supported by Container Apps
- Environment detection: `IsDevelopment()` = local Aspire, `IsProduction()` = Azure
- No new extension class needed - modify existing ServiceCollectionExtensions.cs
- Updated `order.settings.Production.yaml` with Azure Container Apps FQDN for product-service
