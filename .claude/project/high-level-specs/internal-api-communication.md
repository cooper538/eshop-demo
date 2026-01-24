# Internal API Communication

## Metadata

| Attribute | Value |
|-----------|-------|
| Scope | Service-to-service communication layer |
| Services | All microservices that communicate internally |
| Protocol | gRPC (HTTP/2, Protocol Buffers) |
| Routing | Direct service-to-service, NOT via API Gateway |
| Related | [gRPC Communication](./grpc-communication.md) |

---

## 1. Overview

This specification defines the **Internal API** layer - a dedicated communication channel for service-to-service interactions that bypasses the public API Gateway.

### 1.1 API Layer Separation

| Layer | Purpose | Protocol | Routing | Consumers |
|-------|---------|----------|---------|-----------|
| **External API** | Client-facing endpoints | REST/JSON | Via API Gateway (YARP) | Mobile apps, web clients, third parties |
| **Internal API** | Service-to-service | gRPC | Direct, no Gateway | Other microservices only |

```
┌─────────────────────────────────────────────────────────────────┐
│                        EXTERNAL CLIENTS                          │
│              (Mobile, Web, Third-party integrations)             │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             │ HTTPS/REST (External API)
                             ▼
                    ┌─────────────────┐
                    │   API Gateway   │  ← Rate limiting, auth, routing
                    │     (YARP)      │
                    └────────┬────────┘
                             │
           ┌─────────────────┴─────────────────┐
           │                                   │
           ▼                                   ▼
    ┌─────────────┐                     ┌─────────────┐
    │   Product   │◄───────────────────►│    Order    │
    │   Service   │   Internal API      │   Service   │
    │             │   (gRPC)            │             │
    └─────────────┘                     └─────────────┘
           │                                   │
           │         Internal API              │
           │         (direct, no gateway)      │
           └───────────────────────────────────┘
```

### 1.2 Why Separate Internal API?

| Benefit | Description |
|---------|-------------|
| **Performance** | gRPC uses HTTP/2 with binary serialization, much faster than REST/JSON |
| **Strong contracts** | Protocol Buffers enforce schema at compile time |
| **Security isolation** | Internal endpoints not exposed to public internet |
| **Different SLAs** | Internal calls can have different timeouts, retry policies |
| **Simpler contracts** | No need for API versioning, backwards compatibility concerns |

---

## 2. Routing Patterns

### 2.1 URL Conventions

| API Layer | Protocol | Example |
|-----------|----------|---------|
| External API | REST | `GET /api/products` |
| Internal API | gRPC | `ProductService.ReserveStock()` |

### 2.2 Endpoint Visibility

```csharp
// External API - visible in Swagger, routed via Gateway
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProducts() { ... }
}

// Internal API - gRPC service, not in Swagger
public class ProductGrpcService : ProductService.ProductServiceBase
{
    public override Task<ReserveStockResponse> ReserveStock(
        ReserveStockRequest request, ServerCallContext context) { ... }
}
```

### 2.3 API Gateway Configuration (YARP)

The API Gateway routes only External API endpoints:

```json
{
  "ReverseProxy": {
    "Routes": {
      "products-route": {
        "ClusterId": "product-cluster",
        "Match": {
          "Path": "/api/products/{**catch-all}"
        }
      }
      // Note: gRPC endpoints are not routed through Gateway
    }
  }
}
```

---

## 3. Service Discovery

### 3.1 Aspire Service Discovery

In development, .NET Aspire handles service discovery automatically:

```csharp
// EShop.AppHost/Program.cs
var productService = builder.AddProject<Projects.Product_API>("product-service");
var orderService = builder.AddProject<Projects.Order_API>("order-service")
    .WithReference(productService);  // Order can discover Product
```

### 3.2 Configuration-Based Discovery

Services discover each other via configuration:

```json
// Order.API/appsettings.json
{
  "ServiceClients": {
    "ProductService": {
      "Url": "https://product-service:5051"
    }
  }
}
```

### 3.3 Environment-Specific URLs

| Environment | Discovery Method |
|-------------|------------------|
| Development (Aspire) | Automatic via Aspire service references |
| Docker Compose | Service names as hostnames (`https://product-service:5051`) |
| Kubernetes | Kubernetes Service DNS (`https://product-service.namespace.svc.cluster.local:5051`) |

---

## 4. Security Considerations

### 4.1 Network Isolation

Internal API endpoints should be network-isolated:

| Environment | Isolation Method |
|-------------|------------------|
| Docker Compose | Internal Docker network, only Gateway exposed |
| Kubernetes | Network policies, ClusterIP services |
| Cloud | VPC, private subnets, security groups |

```yaml
# docker-compose.yml example
services:
  gateway:
    ports:
      - "5000:5000"  # Only gateway exposed
    networks:
      - frontend
      - backend

  product-service:
    networks:
      - backend  # Internal only

  order-service:
    networks:
      - backend  # Internal only

networks:
  frontend:
  backend:
    internal: true  # Not accessible from host
```

### 4.2 Authentication Between Services

For this demo, internal services trust each other (no authentication). In production:

| Option | Description |
|--------|-------------|
| **mTLS** | Mutual TLS with service certificates |
| **JWT propagation** | Forward user JWT to downstream services |
| **Service mesh** | Istio/Linkerd handles service identity |

### 4.3 No API Gateway Protection

Internal API endpoints lack Gateway protections:
- No rate limiting
- No request validation at Gateway level
- No centralized logging/metrics

**Mitigation**: Implement these at service level where needed.

---

## 5. CorrelationId Propagation

All internal API calls must propagate CorrelationId for distributed tracing.

### 5.1 gRPC Metadata

```csharp
var metadata = new Metadata
{
    { "x-correlation-id", correlationId }
};
await client.ReserveStockAsync(request, metadata);
```

See [CorrelationId Flow](./correlation-id-flow.md) for complete implementation details.

---

## 6. Error Handling

### 6.1 Unified Error Model

Internal API uses `ServiceClientException` for protocol-agnostic errors:

```csharp
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

### 6.2 gRPC to Error Code Mapping

| gRPC StatusCode | ServiceClientErrorCode | HTTP Response to Client |
|-----------------|------------------------|-------------------------|
| `NotFound` | `NotFound` | 404 Not Found |
| `InvalidArgument` | `ValidationError` | 400 Bad Request |
| `Unavailable` | `ServiceUnavailable` | 503 Service Unavailable |
| `DeadlineExceeded` | `Timeout` | 504 Gateway Timeout |

---

## 7. Monitoring and Observability

### 7.1 Metrics to Track

| Metric | Description |
|--------|-------------|
| `grpc_server_handled_total` | Total gRPC calls by service, method, status |
| `grpc_server_handling_seconds` | Latency histogram |
| `grpc_client_handled_total` | Client-side call metrics |

### 7.2 Distributed Tracing

Internal API calls are automatically traced via OpenTelemetry:

```
[Order.API] POST /api/orders
  └─[Order.Application] CreateOrderCommandHandler
      └─[Internal API] ProductService.ReserveStock (gRPC)
          └─[Product.API] ReserveStockCommandHandler
```

---

## Related Documents

- [gRPC Communication](./grpc-communication.md) - gRPC technical patterns
- [CorrelationId Flow](./correlation-id-flow.md) - Distributed tracing
- [Product Service Interface](./product-service-interface.md) - Internal API contracts (server)
- [Order Service Interface](./order-service-interface.md) - Internal API dependencies (client)
- [Aspire Orchestration](./aspire-orchestration.md) - Service discovery
