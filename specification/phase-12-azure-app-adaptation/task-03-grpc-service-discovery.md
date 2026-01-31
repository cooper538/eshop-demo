# Task 03: gRPC Service Discovery

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | pending |
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
// GrpcServiceDiscoveryExtensions.cs
public static class GrpcServiceDiscoveryExtensions
{
    public static IServiceCollection AddGrpcClientAzure<TClient>(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
        where TClient : class
    {
        var isAzure = configuration.GetValue<bool>("Azure:Enabled");

        if (isAzure)
        {
            var baseUri = configuration[$"Services:{serviceName}:Url"]
                ?? throw new InvalidOperationException(
                    $"Service URL for {serviceName} not configured");

            services.AddGrpcClient<TClient>(options =>
            {
                options.Address = new Uri(baseUri);
            })
            .ConfigureChannel(options =>
            {
                // Container Apps handles TLS termination
                options.HttpHandler = new SocketsHttpHandler
                {
                    EnableMultipleHttp2Connections = true
                };
            });
        }
        else
        {
            // Local development - use Aspire service discovery
            services.AddGrpcClient<TClient>(options =>
            {
                options.Address = new Uri($"https://{serviceName}");
            });
        }

        return services;
    }
}
```

## Service URL Configuration (Azure)

```yaml
# appsettings.Azure.yaml
Azure:
  Enabled: true

Services:
  product-service:
    Url: https://product-service.internal.eshop-env.westeurope.azurecontainerapps.io
  order-service:
    Url: https://order-service.internal.eshop-env.westeurope.azurecontainerapps.io
```

## Files to Create/Modify

| Action | File |
|--------|------|
| CREATE | `src/Common/EShop.Common.Infrastructure/Grpc/GrpcServiceDiscoveryExtensions.cs` |
| CREATE | `src/Services/Order/Order.API/appsettings.Azure.yaml` |
| CREATE | `src/Services/Gateway/Gateway.API/appsettings.Azure.yaml` |

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 8.2 Service Discovery)
- -> [internal-api-communication.md](../high-level-specs/internal-api-communication.md) (gRPC configuration)

---
## Notes
- Container Apps internal communication uses HTTPS by default
- gRPC requires HTTP/2 which is supported by Container Apps
