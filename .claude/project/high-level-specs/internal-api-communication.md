# Internal API Communication

## Metadata

| Attribute | Value |
|-----------|-------|
| Scope | Service-to-service communication layer |
| Services | All microservices that communicate internally |
| Protocols | gRPC (primary), HTTP/REST (fallback) |
| Routing | Direct service-to-service, NOT via API Gateway |
| Related | [gRPC Communication](./grpc-communication.md), [Dual-Protocol Communication](./dual-protocol-communication.md) |

---

## 1. Overview

This specification defines the **Internal API** layer - a dedicated communication channel for service-to-service interactions that bypasses the public API Gateway.

### 1.1 API Layer Separation

| Layer | Purpose | Protocol | Routing | Consumers |
|-------|---------|----------|---------|-----------|
| **External API** | Client-facing endpoints | REST/JSON | Via API Gateway (YARP) | Mobile apps, web clients, third parties |
| **Internal API** | Service-to-service | gRPC or REST | Direct, no Gateway | Other microservices only |

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
    │             │   (gRPC/HTTP)       │             │
    └─────────────┘                     └─────────────┘
           │                                   │
           │         Internal API              │
           │         (direct, no gateway)      │
           └───────────────────────────────────┘
```

### 1.2 Why Separate Internal API?

| Benefit | Description |
|---------|-------------|
| **Performance** | Direct communication avoids Gateway overhead |
| **Protocol flexibility** | Internal API can use gRPC (HTTP/2, binary) while External uses REST |
| **Security isolation** | Internal endpoints not exposed to public internet |
| **Different SLAs** | Internal calls can have different timeouts, retry policies |
| **Simpler contracts** | No need for API versioning, backwards compatibility concerns |

---

## 2. Routing Patterns

### 2.1 URL Conventions

| API Layer | URL Pattern | Example |
|-----------|-------------|---------|
| External API | `/api/{resource}` | `GET /api/products` |
| Internal API (REST) | `/internal/{resource}` | `POST /internal/stock/reserve` |
| Internal API (gRPC) | gRPC service endpoint | `ProductService.ReserveStock()` |

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

// Internal API - hidden from Swagger, direct access only
[ApiController]
[Route("internal/stock")]
[ApiExplorerSettings(IgnoreApi = true)]  // Hidden from Swagger
public class InternalStockController : ControllerBase
{
    [HttpPost("reserve")]
    public async Task<IActionResult> ReserveStock() { ... }
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
      // Note: No routes for /internal/* - these are not exposed
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
      "GrpcUrl": "https://product-service:5051",
      "HttpUrl": "https://product-service:5001"
    }
  }
}
```

### 3.3 Environment-Specific URLs

| Environment | Discovery Method |
|-------------|------------------|
| Development (Aspire) | Automatic via Aspire service references |
| Docker Compose | Service names as hostnames (`http://product-service:5001`) |
| Kubernetes | Kubernetes Service DNS (`http://product-service.namespace.svc.cluster.local`) |

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

### 5.1 HTTP Headers

```
X-Correlation-Id: 550e8400-e29b-41d4-a716-446655440000
```

### 5.2 gRPC Metadata

```csharp
var metadata = new Metadata
{
    { "x-correlation-id", correlationId }
};
await client.ReserveStockAsync(request, metadata);
```

See [CorrelationId Flow](./correlation-id-flow.md) for complete implementation details.

---

## 6. Protocol Selection

### 6.1 When to Use gRPC

| Use Case | Reason |
|----------|--------|
| High-throughput calls | Binary protocol, HTTP/2 multiplexing |
| Strongly-typed contracts | Protocol Buffers enforce schema |
| Streaming data | Native streaming support |
| Deadline propagation | Built-in timeout handling |

### 6.2 When to Use HTTP/REST

| Use Case | Reason |
|----------|--------|
| Debugging | Human-readable JSON, easy to inspect |
| Testing | Simple curl/Postman requests |
| Legacy integration | When gRPC not available |
| Load balancer limitations | Some LBs don't support HTTP/2 |

### 6.3 Dual-Protocol Approach

This project uses a **dual-protocol abstraction** that allows switching between gRPC and HTTP via configuration. See [Dual-Protocol Communication](./dual-protocol-communication.md) for implementation details.

---

## 7. Error Handling

### 7.1 Unified Error Model

Internal API uses `ServiceClientException` for all protocol-agnostic errors:

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

### 7.2 Error Propagation

| Source Error | ServiceClientErrorCode | HTTP Response to Client |
|--------------|------------------------|-------------------------|
| gRPC `NotFound` | `NotFound` | 404 Not Found |
| gRPC `InvalidArgument` | `ValidationError` | 400 Bad Request |
| gRPC `Unavailable` | `ServiceUnavailable` | 503 Service Unavailable |
| HTTP 404 | `NotFound` | 404 Not Found |
| HTTP 503 | `ServiceUnavailable` | 503 Service Unavailable |

---

## 8. Monitoring and Observability

### 8.1 Metrics to Track

| Metric | Description |
|--------|-------------|
| `internal_api_requests_total` | Total internal API calls by service, method |
| `internal_api_request_duration_seconds` | Latency histogram |
| `internal_api_errors_total` | Error count by error code |

### 8.2 Distributed Tracing

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
- [Dual-Protocol Communication](./dual-protocol-communication.md) - Protocol abstraction layer
- [CorrelationId Flow](./correlation-id-flow.md) - Distributed tracing
- [Product Service Interface](./product-service-interface.md) - Internal API contracts (server)
- [Order Service Interface](./order-service-interface.md) - Internal API dependencies (client)
- [Aspire Orchestration](./aspire-orchestration.md) - Service discovery
