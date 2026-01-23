# Dual-Protocol Communication

## Metadata

| Attribute | Value |
|-----------|-------|
| Scope | Protocol abstraction for Internal API |
| Services | All services that communicate with other services |
| Protocols | gRPC (HTTP/2, Protocol Buffers) + HTTP/REST (JSON) |
| Package | `EShop.ServiceClients` |
| Configuration | Startup-time via `appsettings.json` feature flag |
| Related | [Internal API Communication](./internal-api-communication.md), [gRPC Communication](./grpc-communication.md) |

---

> **Note**: This document describes the **dual-protocol abstraction pattern**. For specific client implementations and contracts, see [Product Service Interface](./product-service-interface.md) and [Order Service Interface](./order-service-interface.md).

## 1. Overview

This specification defines a **dual-protocol architecture** for Internal API communication. It allows switching between **gRPC** and **HTTP/REST** via configuration, providing flexibility for debugging, testing, and environments where gRPC might not be suitable.

### 1.1 Design Goals

| Goal | Description |
|------|-------------|
| **Protocol Agnostic** | Business logic is unaware of underlying protocol |
| **Configuration-Driven** | Protocol selection via `appsettings.json` |
| **Consistent Resilience** | Same Polly policies for both protocols |
| **Unified Error Handling** | Single exception model regardless of protocol |
| **Easy Testing** | Mock interface, not protocol-specific clients |

### 1.2 Trade-offs

| Aspect | Consideration |
|--------|---------------|
| **Complexity** | Additional abstraction layer and dual API endpoints |
| **Maintenance** | Both protocols must stay in sync |
| **Features** | HTTP lacks gRPC streaming and deadlines |
| **Performance** | gRPC is faster; HTTP is easier to debug |

---

## 2. Architecture

### 2.1 Project Structure

```
src/Common/
└── EShop.ServiceClients/
    ├── EShop.ServiceClients.csproj
    ├── Abstractions/
    │   ├── IProductServiceClient.cs      # Interface for Product Service
    │   └── IOrderServiceClient.cs        # Interface for Order Service
    ├── Grpc/
    │   ├── GrpcProductServiceClient.cs   # gRPC implementation
    │   └── GrpcOrderServiceClient.cs
    ├── Http/
    │   ├── HttpProductServiceClient.cs   # HTTP implementation
    │   └── HttpOrderServiceClient.cs
    ├── Configuration/
    │   └── ServiceClientOptions.cs
    ├── Exceptions/
    │   └── ServiceClientException.cs
    └── Extensions/
        └── ServiceCollectionExtensions.cs
```

### 2.2 Dependencies

```
EShop.ServiceClients
├── EShop.Contracts          # Shared DTOs
├── EShop.Grpc               # Generated gRPC clients
├── Grpc.Net.Client          # gRPC client
├── Microsoft.Extensions.Http # HttpClientFactory
└── Polly                    # Resilience
```

### 2.3 Abstraction Layer

```
┌─────────────────────────────────────────────────────────────────┐
│                     Consuming Service                            │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                  IProductServiceClient                   │   │
│  │        (Business logic uses this interface)             │   │
│  └─────────────────────────────────────────────────────────┘   │
│                             │                                   │
│              Configuration: "Protocol": "Grpc" | "Http"        │
│                             │                                   │
│         ┌───────────────────┴───────────────────┐              │
│         ▼                                       ▼              │
│  ┌──────────────────┐                ┌──────────────────┐     │
│  │ GrpcProduct      │                │ HttpProduct      │     │
│  │ ServiceClient    │                │ ServiceClient    │     │
│  └──────────────────┘                └──────────────────┘     │
│         │                                       │              │
└─────────│───────────────────────────────────────│──────────────┘
          │                                       │
          ▼                                       ▼
   ┌──────────────┐                      ┌──────────────┐
   │ gRPC Server  │                      │ REST Server  │
   │ (port 5051)  │                      │ /internal/*  │
   └──────────────┘                      └──────────────┘
```

---

## 3. Interface Pattern

### 3.1 Service Client Interface

Each service exposes an interface for its Internal API:

```csharp
namespace EShop.ServiceClients.Abstractions;

/// <summary>
/// Internal API abstraction for {Service} communication.
/// Implementation selected based on configuration (gRPC or HTTP).
/// </summary>
public interface I{Service}ServiceClient
{
    /// <summary>
    /// {Method description}
    /// </summary>
    Task<{Result}> {Method}Async(
        {Request} request,
        CancellationToken cancellationToken = default);
}
```

### 3.2 Request/Response Models

Define protocol-agnostic models in the Abstractions folder:

```csharp
namespace EShop.ServiceClients.Abstractions;

// Request model
public sealed record {Method}Request(
    Guid Id,
    // ... other properties
);

// Result model
public sealed record {Method}Result(
    bool Success,
    string? FailureReason = null
);
```

**Note**: These models are separate from gRPC-generated messages. The implementation maps between them.

---

## 4. Configuration

### 4.1 Options Class

```csharp
namespace EShop.ServiceClients.Configuration;

public sealed class ServiceClientOptions
{
    public const string SectionName = "ServiceClients";

    /// <summary>
    /// Protocol to use for inter-service communication.
    /// Valid values: "Grpc", "Http"
    /// </summary>
    public string Protocol { get; set; } = "Grpc";

    /// <summary>
    /// Service endpoints configuration.
    /// </summary>
    public ServiceEndpoints ProductService { get; set; } = new();
    public ServiceEndpoints OrderService { get; set; } = new();
}

public sealed class ServiceEndpoints
{
    public string GrpcUrl { get; set; } = string.Empty;
    public string HttpUrl { get; set; } = string.Empty;
}
```

### 4.2 appsettings.json

```json
{
  "ServiceClients": {
    "Protocol": "Grpc",
    "ProductService": {
      "GrpcUrl": "https://localhost:5051",
      "HttpUrl": "https://localhost:5001"
    },
    "OrderService": {
      "GrpcUrl": "https://localhost:5052",
      "HttpUrl": "https://localhost:5002"
    }
  }
}
```

### 4.3 Environment-Specific Overrides

```json
// appsettings.Development.json - use HTTP for easier debugging
{
  "ServiceClients": {
    "Protocol": "Http"
  }
}

// appsettings.Production.json - use gRPC for performance
{
  "ServiceClients": {
    "Protocol": "Grpc"
  }
}
```

---

## 5. Implementation Pattern

### 5.1 gRPC Implementation Pattern

```csharp
namespace EShop.ServiceClients.Grpc;

public sealed class Grpc{Service}ServiceClient : I{Service}ServiceClient
{
    private readonly {Service}Service.{Service}ServiceClient _client;
    private readonly ILogger<Grpc{Service}ServiceClient> _logger;

    public Grpc{Service}ServiceClient(
        {Service}Service.{Service}ServiceClient client,
        ILogger<Grpc{Service}ServiceClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<{Result}> {Method}Async(
        {Request} request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Calling {Method} via gRPC", nameof({Method}Async));

        try
        {
            // Map domain request to gRPC request
            var grpcRequest = MapToGrpcRequest(request);

            // Call with deadline
            var response = await _client.{Method}Async(
                grpcRequest,
                deadline: DateTime.UtcNow.AddSeconds(30),
                cancellationToken: cancellationToken);

            // Map gRPC response to domain result
            return MapToResult(response);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error in {Method}", nameof({Method}Async));
            throw new ServiceClientException(
                $"Failed to call {Method}: {ex.Status.Detail}",
                ex,
                MapGrpcStatus(ex.StatusCode));
        }
    }

    private static ServiceClientErrorCode MapGrpcStatus(StatusCode statusCode) => statusCode switch
    {
        StatusCode.NotFound => ServiceClientErrorCode.NotFound,
        StatusCode.InvalidArgument => ServiceClientErrorCode.ValidationError,
        StatusCode.Unavailable => ServiceClientErrorCode.ServiceUnavailable,
        StatusCode.DeadlineExceeded => ServiceClientErrorCode.Timeout,
        _ => ServiceClientErrorCode.Unknown
    };
}
```

### 5.2 HTTP Implementation Pattern

```csharp
namespace EShop.ServiceClients.Http;

public sealed class Http{Service}ServiceClient : I{Service}ServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<Http{Service}ServiceClient> _logger;

    public Http{Service}ServiceClient(
        HttpClient httpClient,
        ILogger<Http{Service}ServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<{Result}> {Method}Async(
        {Request} request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Calling {Method} via HTTP", nameof({Method}Async));

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "internal/{resource}/{action}",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(cancellationToken);
                throw new ServiceClientException(
                    error?.Message ?? "Unknown error",
                    null,
                    MapHttpStatus(response.StatusCode));
            }

            return await response.Content.ReadFromJsonAsync<{Result}>(cancellationToken)
                ?? throw new ServiceClientException("Empty response", null, ServiceClientErrorCode.Unknown);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error in {Method}", nameof({Method}Async));
            throw new ServiceClientException(
                $"Failed to call {Method}: {ex.Message}",
                ex,
                ServiceClientErrorCode.ServiceUnavailable);
        }
    }

    private static ServiceClientErrorCode MapHttpStatus(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.NotFound => ServiceClientErrorCode.NotFound,
        HttpStatusCode.BadRequest => ServiceClientErrorCode.ValidationError,
        HttpStatusCode.ServiceUnavailable => ServiceClientErrorCode.ServiceUnavailable,
        HttpStatusCode.RequestTimeout => ServiceClientErrorCode.Timeout,
        _ => ServiceClientErrorCode.Unknown
    };
}
```

---

## 6. Unified Exception Handling

### 6.1 ServiceClientException

```csharp
namespace EShop.ServiceClients.Exceptions;

/// <summary>
/// Unified exception for service client errors, regardless of protocol.
/// </summary>
public sealed class ServiceClientException : Exception
{
    public ServiceClientErrorCode ErrorCode { get; }

    public ServiceClientException(
        string message,
        Exception? innerException,
        ServiceClientErrorCode errorCode)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

public enum ServiceClientErrorCode
{
    Unknown = 0,
    NotFound = 1,
    ValidationError = 2,
    ServiceUnavailable = 3,
    Timeout = 4,
    Unauthorized = 5
}
```

### 6.2 Error Mapping

| gRPC StatusCode | HTTP StatusCode | ServiceClientErrorCode |
|-----------------|-----------------|------------------------|
| `NotFound` | `404 Not Found` | `NotFound` |
| `InvalidArgument` | `400 Bad Request` | `ValidationError` |
| `Unavailable` | `503 Service Unavailable` | `ServiceUnavailable` |
| `DeadlineExceeded` | `408 Request Timeout` | `Timeout` |
| `PermissionDenied` | `401/403` | `Unauthorized` |
| Other | Other | `Unknown` |

---

## 7. DI Registration

### 7.1 Extension Method Pattern

```csharp
namespace EShop.ServiceClients.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(ServiceClientOptions.SectionName)
            .Get<ServiceClientOptions>()
            ?? new ServiceClientOptions();

        services.Configure<ServiceClientOptions>(
            configuration.GetSection(ServiceClientOptions.SectionName));

        var useGrpc = options.Protocol.Equals("Grpc", StringComparison.OrdinalIgnoreCase);

        if (useGrpc)
        {
            RegisterGrpcClients(services, options);
        }
        else
        {
            RegisterHttpClients(services, options);
        }

        return services;
    }

    private static void RegisterGrpcClients(
        IServiceCollection services,
        ServiceClientOptions options)
    {
        // Register gRPC client with resilience policies
        services.AddGrpcClient<{Service}Service.{Service}ServiceClient>(o =>
        {
            o.Address = new Uri(options.{Service}Service.GrpcUrl);
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                handler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }
            return handler;
        })
        .AddResilienceHandler();

        services.AddScoped<I{Service}ServiceClient, Grpc{Service}ServiceClient>();
    }

    private static void RegisterHttpClients(
        IServiceCollection services,
        ServiceClientOptions options)
    {
        services.AddHttpClient<I{Service}ServiceClient, Http{Service}ServiceClient>(client =>
        {
            client.BaseAddress = new Uri(options.{Service}Service.HttpUrl);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddResilienceHandler();
    }

    private static IHttpClientBuilder AddResilienceHandler(this IHttpClientBuilder builder)
    {
        return builder
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * 100));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }
}
```

### 7.2 Usage in Services

```csharp
// Order.API/Program.cs
builder.Services.AddServiceClients(builder.Configuration);

// Order.Application/Handlers/CreateOrderCommandHandler.cs
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResult>
{
    private readonly IProductServiceClient _productClient;

    public CreateOrderCommandHandler(IProductServiceClient productClient)
    {
        _productClient = productClient;
    }

    public async Task<OrderResult> Handle(CreateOrderCommand command, CancellationToken ct)
    {
        // Same code works with both gRPC and HTTP!
        var result = await _productClient.ReserveStockAsync(
            new ReserveStockRequest(command.OrderId, command.Items),
            ct);

        if (!result.Success)
        {
            return OrderResult.Rejected(result.FailureReason);
        }

        return OrderResult.Confirmed();
    }
}
```

---

## 8. Testing

### 8.1 Unit Tests (Mock Interface)

```csharp
public class CreateOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenStockAvailable_ReturnsConfirmed()
    {
        // Arrange - mock the interface, not the protocol
        var productClient = new Mock<IProductServiceClient>();
        productClient
            .Setup(x => x.ReserveStockAsync(It.IsAny<ReserveStockRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StockReservationResult(true));

        var handler = new CreateOrderCommandHandler(productClient.Object);

        // Act
        var result = await handler.Handle(new CreateOrderCommand(...), CancellationToken.None);

        // Assert
        result.Should().BeConfirmed();
    }
}
```

### 8.2 Integration Tests (Both Protocols)

```csharp
public class ProductServiceClientIntegrationTests : IClassFixture<ProductServiceFixture>
{
    [Theory]
    [InlineData("Grpc")]
    [InlineData("Http")]
    public async Task ReserveStock_WithAvailableStock_ReturnsSuccess(string protocol)
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ServiceClients:Protocol"] = protocol,
                ["ServiceClients:ProductService:GrpcUrl"] = _fixture.GrpcUrl,
                ["ServiceClients:ProductService:HttpUrl"] = _fixture.HttpUrl,
            })
            .Build();

        services.AddServiceClients(config);
        var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IProductServiceClient>();

        // Act
        var result = await client.ReserveStockAsync(new ReserveStockRequest(...));

        // Assert
        result.Success.Should().BeTrue();
    }
}
```

### 8.3 CI Matrix Testing

```yaml
# .github/workflows/ci.yml
jobs:
  test:
    strategy:
      matrix:
        protocol: [Grpc, Http]
    steps:
      - name: Run integration tests
        env:
          ServiceClients__Protocol: ${{ matrix.protocol }}
        run: dotnet test --filter "Category=Integration"
```

---

## 9. Risks and Mitigations

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| **Semantic differences** | High | Medium | Define abstraction at common denominator; avoid gRPC-specific features in interface |
| **Error handling inconsistency** | High | Medium | Unified `ServiceClientException` with protocol-agnostic error codes |
| **Serialization edge cases** | Medium | Medium | Integration tests with edge case data (nulls, empty strings, special characters) |
| **Dual API maintenance** | Medium | High | Both endpoints call same MediatR handlers; share validation logic |
| **Performance divergence** | Low | High | Document expected differences; benchmark both protocols |
| **Configuration errors** | Low | Low | Startup validation; health checks for both endpoints |

---

## 10. Migration Path

If starting with gRPC-only and adding HTTP later:

1. Add `EShop.ServiceClients` project with interface
2. Wrap existing gRPC client in `Grpc{Service}ServiceClient`
3. Update DI registration to use interface
4. Add REST controllers to target service (`/internal/*`)
5. Implement `Http{Service}ServiceClient`
6. Add protocol configuration

**Estimated effort**: 1-2 days for existing service.

---

## Related Documents

- [Internal API Communication](./internal-api-communication.md) - Internal API layer concept
- [gRPC Communication](./grpc-communication.md) - gRPC-specific patterns
- [Product Service Interface](./product-service-interface.md) - ProductServiceClient implementation
- [Order Service Interface](./order-service-interface.md) - OrderServiceClient implementation
- [Unit Testing](./unit-testing.md) - Testing patterns
- [Functional Testing](./functional-testing.md) - Integration test setup
