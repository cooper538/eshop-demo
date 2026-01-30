# gRPC Communication Patterns

## Metadata

| Attribute | Value |
|-----------|-------|
| Scope | gRPC technical implementation patterns |
| Protocol | HTTP/2, Protocol Buffers |
| Package | `EShop.Grpc` |
| Related | [Internal API Communication](./internal-api-communication.md) |

---

> **Note**: This document describes **technical patterns** for gRPC implementation. For specific service contracts and endpoints, see [Product Service Interface](./product-service-interface.md) and [Order Service Interface](./order-service-interface.md).

> **Reference**: For API design best practices, see [Google API Improvement Proposals (AIP)](https://google.aip.dev/) - especially [AIP-231 (Batch Get)](https://google.aip.dev/231) and [AIP-193 (Errors)](https://google.aip.dev/193).

## 1. Overview

gRPC is the **primary protocol** for the Internal API layer - synchronous communication between microservices. This provides:

| Feature | Benefit |
|---------|---------|
| **Strong typing** | Protocol Buffers enforce schema at compile time |
| **HTTP/2** | Efficient binary transport, multiplexing |
| **Streaming support** | Bi-directional streaming (not used in this demo) |
| **Built-in deadlines** | Timeout propagation across services |
| **Code generation** | Automatic client/server stubs |

### 1.1 When to Use gRPC

| Use Case | gRPC | HTTP/REST |
|----------|------|-----------|
| High-throughput internal calls | ✓ | |
| Strongly-typed contracts | ✓ | |
| Debugging/testing | | ✓ |
| Legacy integration | | ✓ |
| Browser clients | | ✓ |

---

## 2. Project Setup

### 2.1 Proto File Organization

```
src/Common/EShop.Grpc/
├── EShop.Grpc.csproj
└── Protos/
    ├── product.proto      # Product Service definitions
    ├── order.proto        # Order Service definitions
    └── shared/
        └── common.proto   # Shared message types (optional)
```

### 2.2 Project Configuration

```xml
<!-- EShop.Grpc.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <!-- Generate both client and server code -->
    <Protobuf Include="Protos\**\*.proto" GrpcServices="Both" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Grpc.AspNetCore" Version="2.x" />
  </ItemGroup>
</Project>
```

### 2.3 Proto File Best Practices

```protobuf
syntax = "proto3";

// C# namespace for generated code
option csharp_namespace = "EShop.Grpc.{ServiceName}";

package {servicename};

// Service definition
service {ServiceName}Service {
  rpc {MethodName} ({MethodName}Request) returns ({MethodName}Response);
}

// Messages
message {MethodName}Request {
  // Fields with explicit numbers
  string field_name = 1;
  int32 quantity = 2;
}
```

| Best Practice | Description |
|---------------|-------------|
| **Explicit field numbers** | Never reuse numbers, even after deleting fields |
| **snake_case for fields** | Proto convention, auto-converts to PascalCase in C# |
| **Decimal as string** | Protocol Buffers lacks native decimal; string preserves precision |
| **No CorrelationId in messages** | Propagate via gRPC metadata (interceptors) |

### 2.4 Generated Code

Grpc.Tools automatically generates:
- `{ServiceName}Service.{ServiceName}ServiceClient` - for calling service
- `{ServiceName}Service.{ServiceName}ServiceBase` - for implementing service
- Message classes for all request/response types

---

## 3. Server-Side Patterns

### 3.1 Service Registration

```csharp
// Program.cs (server)
var builder = WebApplication.CreateBuilder(args);

// Add gRPC services
builder.Services.AddGrpc(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors = true;  // Expose stack traces in dev
    }
});

var app = builder.Build();

// Map gRPC endpoints
app.MapGrpcService<ProductGrpcService>();

app.Run();
```

### 3.2 Service Implementation Pattern

```csharp
public class YourGrpcService : YourService.YourServiceBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<YourGrpcService> _logger;

    public YourGrpcService(IMediator mediator, ILogger<YourGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<YourResponse> YourMethod(
        YourRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("YourMethod called with {Parameter}", request.Parameter);

        // Delegate to MediatR handler (same as REST endpoint)
        var query = new YourQuery(request.Parameter);
        var result = await _mediator.Send(query, context.CancellationToken);

        // Map domain result to gRPC response
        return MapToResponse(result);
    }
}
```

**Key points:**
- Delegate to MediatR handlers for consistent behavior with REST endpoints
- Use `context.CancellationToken` for cancellation propagation
- Log at Debug level for request details

### 3.3 Exception Interceptor

Convert domain exceptions to gRPC status codes:

```csharp
public class GrpcExceptionInterceptor : Interceptor
{
    private readonly ILogger<GrpcExceptionInterceptor> _logger;

    public GrpcExceptionInterceptor(ILogger<GrpcExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (RpcException)
        {
            throw;  // Already gRPC exception, rethrow
        }
        catch (NotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (ValidationException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (ConflictException ex)
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in gRPC call {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }
}
```

### 3.4 Exception to gRPC Status Mapping

| Domain Exception | gRPC Status Code | Description |
|------------------|------------------|-------------|
| `NotFoundException` | `NotFound` | Resource not found |
| `ValidationException` | `InvalidArgument` | Invalid request data |
| `ConflictException` | `AlreadyExists` | Resource already exists |
| `UnauthorizedException` | `PermissionDenied` | Access denied |
| Other exceptions | `Internal` | Unexpected server error |

### 3.5 Interceptor Registration

```csharp
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<GrpcExceptionInterceptor>();
    options.Interceptors.Add<CorrelationIdInterceptor>();  // See CorrelationId Flow
});
```

---

## 4. Client-Side Patterns

### 4.1 Client Registration

```csharp
// Program.cs (client service)
builder.Services.AddGrpcClient<YourService.YourServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcServices:YourService"]!);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    if (builder.Environment.IsDevelopment())
    {
        // WARNING: Only for development - bypasses SSL validation
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }
    return handler;
});
```

### 4.2 Client Wrapper Pattern

Wrap generated client for cleaner API:

```csharp
public class YourServiceGrpcClient : IYourServiceClient
{
    private readonly YourService.YourServiceClient _client;
    private readonly ILogger<YourServiceGrpcClient> _logger;

    public YourServiceGrpcClient(
        YourService.YourServiceClient client,
        ILogger<YourServiceGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<YourResult> YourMethodAsync(
        YourDomainRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Calling YourMethod with {Parameter}", request.Parameter);

        // Map domain request to gRPC request
        var grpcRequest = new YourRequest { Parameter = request.Parameter };

        // Call with deadline
        var response = await _client.YourMethodAsync(
            grpcRequest,
            deadline: DateTime.UtcNow.AddSeconds(30),
            cancellationToken: cancellationToken);

        // Map gRPC response to domain result
        return MapToResult(response);
    }
}
```

### 4.3 Always Set Deadlines

Every gRPC call should include a deadline to prevent infinite waiting:

```csharp
var response = await _client.YourMethodAsync(
    request,
    deadline: DateTime.UtcNow.AddSeconds(30),  // Always set deadline
    cancellationToken: cancellationToken);
```

---

## 5. Resiliency Patterns

### 5.1 Retry Policy (Polly)

```csharp
builder.Services
    .AddGrpcClient<YourService.YourServiceClient>(options => { ... })
    .AddPolicyHandler(GetRetryPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.ServiceUnavailable)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt =>
                TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * 100));
                // Exponential backoff: 100ms, 200ms, 400ms
}
```

### 5.2 Circuit Breaker

```csharp
.AddPolicyHandler(GetCircuitBreakerPolicy());

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30));
}
```

### 5.3 Failure Scenarios

| Scenario | Behavior | Recommended Response |
|----------|----------|---------------------|
| Service timeout | Retry 3x with backoff, then fail | 503 Service Unavailable |
| Service down (circuit open) | Immediate fail | 503 Service Unavailable |
| Business logic failure | No retry (not transient) | 200 OK with error in response |
| Invalid request | No retry | 400 Bad Request |

---

## 6. Observability

### 6.1 OpenTelemetry gRPC Instrumentation

```csharp
// EShop.Common/Extensions/OpenTelemetryExtensions.cs
public static IHostApplicationBuilder ConfigureOpenTelemetry(
    this IHostApplicationBuilder builder)
{
    builder.Services.AddOpenTelemetry()
        .WithTracing(tracing =>
        {
            tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddGrpcClientInstrumentation(options =>
                {
                    options.SuppressDownstreamInstrumentation = false;
                    options.EnrichWithHttpRequestMessage = (activity, request) =>
                    {
                        activity.SetTag("grpc.request.uri", request.RequestUri?.ToString());
                    };
                });

            // Export to configured sink (Seq, Jaeger, etc.)
            if (!string.IsNullOrEmpty(builder.Configuration["Seq:ServerUrl"]))
            {
                tracing.AddOtlpExporter(o =>
                    o.Endpoint = new Uri(builder.Configuration["Seq:ServerUrl"]!));
            }
        });

    return builder;
}
```

**Required Package:**
```xml
<PackageReference Include="OpenTelemetry.Instrumentation.GrpcNetClient" Version="1.x" />
```

### 6.2 Structured Logging

```csharp
_logger.LogInformation(
    "gRPC call {Method} completed in {Duration}ms with status {Status}",
    context.Method,
    stopwatch.ElapsedMilliseconds,
    response.Success ? "Success" : "Failure");
```

---

## 7. Testing

### 7.1 TestServerCallContext Helper

For unit testing gRPC services, use a test `ServerCallContext`:

```csharp
public static class TestServerCallContext
{
    public static ServerCallContext Create(
        CancellationToken cancellationToken = default)
    {
        return new TestServerCallContextImpl(cancellationToken);
    }

    private class TestServerCallContextImpl : ServerCallContext
    {
        private readonly CancellationToken _cancellationToken;
        private readonly Metadata _requestHeaders = new();
        private readonly Metadata _responseTrailers = new();

        public TestServerCallContextImpl(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        protected override CancellationToken CancellationTokenCore => _cancellationToken;
        protected override Metadata RequestHeadersCore => _requestHeaders;
        protected override Metadata ResponseTrailersCore => _responseTrailers;
        protected override string MethodCore => "TestMethod";
        protected override string HostCore => "localhost";
        protected override string PeerCore => "127.0.0.1";
        protected override DateTime DeadlineCore => DateTime.UtcNow.AddMinutes(1);
        protected override WriteOptions? WriteOptionsCore { get; set; }
        protected override AuthContext AuthContextCore =>
            new AuthContext(null, new Dictionary<string, List<AuthProperty>>());

        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
            => Task.CompletedTask;
        protected override ContextPropagationToken CreatePropagationTokenCore(
            ContextPropagationOptions? options) => null!;
    }
}
```

### 7.2 Unit Test Example

```csharp
[Fact]
public async Task YourMethod_WithValidRequest_ReturnsExpectedResult()
{
    // Arrange
    var mediator = new Mock<IMediator>();
    mediator
        .Setup(m => m.Send(It.IsAny<YourQuery>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new YourResult(...));

    var service = new YourGrpcService(mediator.Object, Mock.Of<ILogger<YourGrpcService>>());
    var context = TestServerCallContext.Create();

    // Act
    var response = await service.YourMethod(
        new YourRequest { Parameter = "test" },
        context);

    // Assert
    response.Should().NotBeNull();
    response.Result.Should().Be("expected");
}
```

See [Unit Testing](./unit-testing.md) for complete testing patterns.

---

## Related Documents

- [Internal API Communication](./internal-api-communication.md) - Internal API layer concept
- 
- [Product Service Interface](./product-service-interface.md) - gRPC server implementation (ProductService proto)
- [Order Service Interface](./order-service-interface.md) - gRPC server/client (OrderService proto)
- [CorrelationId Flow](./correlation-id-flow.md) - Request tracing across gRPC calls
- [Unit Testing](./unit-testing.md) - TestServerCallContext helper
- [Functional Testing](./functional-testing.md) - Integration testing patterns
