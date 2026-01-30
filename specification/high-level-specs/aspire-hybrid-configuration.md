# Aspire Hybrid Configuration

## Metadata

| Attribute | Value |
|-----------|-------|
| Scope | Centralized configuration management |
| Approach | Aspire Hybrid (Environment-specific) |
| Applies To | All services |
| Related | [Aspire Orchestration](./aspire-orchestration.md) |

---

## 1. Overview

Aspire Hybrid is a configuration strategy that combines .NET Aspire's built-in capabilities with environment-specific secret management. It provides flexibility to use different configuration sources based on the deployment environment while maintaining a consistent developer experience.

### 1.1 Why Aspire Hybrid for This Project

| Benefit | Description |
|---------|-------------|
| **Leverages Existing Aspire** | Project already uses Aspire for orchestration |
| **Minimal Overhead** | No additional infrastructure (Consul, etcd) required |
| **Environment Flexibility** | Different secret backends per environment |
| **Production-Ready Path** | Easy upgrade to Azure Key Vault or K8s Secrets |
| **Developer Experience** | Simple local development with User Secrets |

### 1.2 Configuration Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    CONFIGURATION LAYERS                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Layer 4: Environment Variables (highest priority)               │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  K8s ConfigMaps/Secrets, Docker env, Aspire injection    │    │
│  └─────────────────────────────────────────────────────────┘    │
│                            ▲                                     │
│  Layer 3: User Secrets / Aspire Parameters                       │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  dotnet user-secrets, builder.AddParameter(secret:true)  │    │
│  └─────────────────────────────────────────────────────────┘    │
│                            ▲                                     │
│  Layer 2: appsettings.{Environment}.json                         │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  appsettings.Development.json, appsettings.Docker.json   │    │
│  └─────────────────────────────────────────────────────────┘    │
│                            ▲                                     │
│  Layer 1: appsettings.json (lowest priority)                     │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  Default values, feature flags, timeouts (checked in git)│    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 2. Configuration Layers

### 2.1 Layer 1: Base Configuration (`appsettings.json`)

Non-sensitive configuration checked into source control.

```json
// appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Features": {
    "EnableNewCheckoutFlow": false,
    "EnableStockNotifications": true
  },
  "Resilience": {
    "GrpcDeadlineSeconds": 30,
    "HttpTimeoutSeconds": 10,
    "MaxRetryAttempts": 3
  },
  "Pagination": {
    "DefaultPageSize": 20,
    "MaxPageSize": 100
  }
}
```

### 2.2 Layer 2: Environment Overrides (`appsettings.{Environment}.json`)

Environment-specific overrides, also checked into git.

```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "Features": {
    "EnableNewCheckoutFlow": true
  }
}
```

### 2.3 Layer 3: Secrets (User Secrets / Aspire Parameters)

Sensitive configuration that should **never** be in source control.

```bash
# Development: User Secrets
dotnet user-secrets set "SendGrid:ApiKey" "SG.xxx" --project src/Services/Notification

# Aspire: Secret Parameters
# Defined in AppHost/Program.cs
```

### 2.4 Layer 4: Environment Variables

Highest priority, used for runtime injection by orchestrators.

```bash
# Set by Aspire automatically
ConnectionStrings__productdb=Host=localhost;Database=productdb;...
ConnectionStrings__messaging=amqp://guest:guest@localhost:5672

# Set by K8s ConfigMaps
Features__EnableNewCheckoutFlow=true
```

---

## 3. Environment-Specific Strategy

| Environment | Non-Sensitive Config | Secrets | Injected By |
|-------------|---------------------|---------|-------------|
| **Development** | appsettings.Development.json | User Secrets | .NET Runtime |
| **Docker Compose** | appsettings.json + .env | Docker Secrets / .env | Docker |
| **Kubernetes** | appsettings.json + ConfigMaps | K8s Secrets / Key Vault | K8s + CSI Driver |
| **Azure** | appsettings.json + App Configuration | Azure Key Vault | Aspire / ACA |

### 3.1 Development Workflow

```bash
# Initialize User Secrets for a service
cd src/Services/Product
dotnet user-secrets init

# Set secrets
dotnet user-secrets set "ExternalApis:SendGrid:ApiKey" "SG.your-api-key"
dotnet user-secrets set "ExternalApis:Stripe:SecretKey" "sk_test_xxx"

# List secrets
dotnet user-secrets list

# Remove secret
dotnet user-secrets remove "ExternalApis:SendGrid:ApiKey"
```

### 3.2 Docker Compose Workflow

```yaml
# docker-compose.override.yml
services:
  product-service:
    environment:
      - ExternalApis__SendGrid__ApiKey=${SENDGRID_API_KEY}
    secrets:
      - stripe_secret

secrets:
  stripe_secret:
    file: ./secrets/stripe_secret.txt
```

### 3.3 Kubernetes Workflow

```yaml
# k8s/product-service-deployment.yaml
spec:
  containers:
    - name: product-service
      envFrom:
        - configMapRef:
            name: product-service-config
        - secretRef:
            name: product-service-secrets
```

---

## 4. Aspire Parameter Configuration

### 4.1 Defining Parameters in AppHost

**File**: `src/AppHost/Program.cs`

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════
// PARAMETERS (Secrets)
// ═══════════════════════════════════════════════════════════════

// Database password - marked as secret
var dbPassword = builder.AddParameter("postgres-password", secret: true);

// External API keys
var sendGridApiKey = builder.AddParameter("sendgrid-api-key", secret: true);

// ═══════════════════════════════════════════════════════════════
// INFRASTRUCTURE
// ═══════════════════════════════════════════════════════════════

var postgres = builder.AddPostgres("postgres")
    .WithPassword(dbPassword)
    .WithLifetime(ContainerLifetime.Persistent);

// ═══════════════════════════════════════════════════════════════
// SERVICES
// ═══════════════════════════════════════════════════════════════

var notificationService = builder.AddProject<Projects.EShop_NotificationService>("notification-service")
    .WithReference(rabbitmq)
    .WithEnvironment("ExternalApis__SendGrid__ApiKey", sendGridApiKey);

builder.Build().Run();
```

### 4.2 Providing Parameter Values

```bash
# Option 1: Environment variables (prefixed with parameter name)
export Parameters__postgres-password="MySecurePassword123"
export Parameters__sendgrid-api-key="SG.xxx"

# Option 2: appsettings.json in AppHost (NOT recommended for secrets)
{
  "Parameters": {
    "postgres-password": "DevPassword123"
  }
}

# Option 3: User Secrets in AppHost
cd src/AppHost
dotnet user-secrets set "Parameters:postgres-password" "MySecurePassword123"
```

---

## 5. Options Pattern Implementation

### 5.1 Configuration Classes

**File**: `src/Services/Product/Product.API/Configuration/ResilienceOptions.cs`

```csharp
namespace EShop.ProductService.Configuration;

public sealed class ResilienceOptions
{
    public const string SectionName = "Resilience";

    public int GrpcDeadlineSeconds { get; init; } = 30;
    public int HttpTimeoutSeconds { get; init; } = 10;
    public int MaxRetryAttempts { get; init; } = 3;
}
```

**File**: `src/Services/Product/Product.API/Configuration/FeatureOptions.cs`

```csharp
namespace EShop.ProductService.Configuration;

public sealed class FeatureOptions
{
    public const string SectionName = "Features";

    public bool EnableNewCheckoutFlow { get; init; }
    public bool EnableStockNotifications { get; init; } = true;
}
```

### 5.2 Registration

**File**: `src/Services/Product/Product.API/Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════
// CONFIGURATION - Options Pattern
// ═══════════════════════════════════════════════════════════════

builder.Services
    .AddOptions<ResilienceOptions>()
    .BindConfiguration(ResilienceOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<FeatureOptions>()
    .BindConfiguration(FeatureOptions.SectionName)
    .ValidateOnStart();

// ═══════════════════════════════════════════════════════════════
// ASPIRE INTEGRATION
// ═══════════════════════════════════════════════════════════════

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<ProductDbContext>("productdb");
```

### 5.3 Usage in Services

```csharp
public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOptions<FeatureOptions> _features;
    private readonly IOptionsSnapshot<ResilienceOptions> _resilience;

    public CreateOrderCommandHandler(
        IOptions<FeatureOptions> features,
        IOptionsSnapshot<ResilienceOptions> resilience)
    {
        _features = features;
        _resilience = resilience;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        if (_features.Value.EnableNewCheckoutFlow)
        {
            // Use new checkout flow
        }

        var timeout = TimeSpan.FromSeconds(_resilience.Value.HttpTimeoutSeconds);
        // ...
    }
}
```

### 5.4 IOptions vs IOptionsSnapshot vs IOptionsMonitor

| Interface | Lifetime | Hot-Reload | Use When |
|-----------|----------|------------|----------|
| `IOptions<T>` | Singleton | No | Config that never changes |
| `IOptionsSnapshot<T>` | Scoped | Yes (per request) | Request-scoped config |
| `IOptionsMonitor<T>` | Singleton | Yes (immediate) | Long-lived services, background workers |

---

## 6. Configuration Categories

| Category | Examples | Storage Location | Hot-Reload |
|----------|----------|-----------------|------------|
| **Feature Flags** | EnableNewCheckout, EnableNotifications | appsettings.json | Yes* |
| **Timeouts** | GrpcDeadline, HttpTimeout | appsettings.json | Yes* |
| **Pagination** | DefaultPageSize, MaxPageSize | appsettings.json | Yes* |
| **Logging** | LogLevel, Sinks | appsettings.json | Yes |
| **Connection Strings** | Databases, Message Broker | Aspire injection | No |
| **API Keys** | SendGrid, Stripe | Secrets (User/K8s/Vault) | No |
| **Service URLs** | N/A | Aspire Service Discovery | N/A |

*Requires `IOptionsSnapshot<T>` or `IOptionsMonitor<T>`

---

## 7. Validation

### 7.1 Data Annotations Validation

```csharp
using System.ComponentModel.DataAnnotations;

public sealed class ResilienceOptions
{
    public const string SectionName = "Resilience";

    [Range(1, 120)]
    public int GrpcDeadlineSeconds { get; init; } = 30;

    [Range(1, 60)]
    public int HttpTimeoutSeconds { get; init; } = 10;

    [Range(0, 10)]
    public int MaxRetryAttempts { get; init; } = 3;
}
```

### 7.2 Custom Validation

```csharp
builder.Services
    .AddOptions<ExternalApiOptions>()
    .BindConfiguration(ExternalApiOptions.SectionName)
    .Validate(options =>
    {
        if (string.IsNullOrEmpty(options.SendGrid.ApiKey))
            return false;

        return options.SendGrid.ApiKey.StartsWith("SG.");
    }, "SendGrid API key must start with 'SG.'")
    .ValidateOnStart();
```

### 7.3 Fail-Fast on Startup

The `ValidateOnStart()` method ensures the application fails immediately if configuration is invalid:

```csharp
// Application will throw on startup if validation fails
builder.Services
    .AddOptions<ResilienceOptions>()
    .BindConfiguration(ResilienceOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();  // <-- Fail-fast
```

---

## 8. Secret Management Best Practices

### 8.1 What Should Be a Secret

| Secret | Not a Secret |
|--------|--------------|
| Database passwords | Database host/port |
| API keys (SendGrid, Stripe) | API base URLs |
| JWT signing keys | JWT issuer/audience |
| Encryption keys | Encryption algorithm |
| OAuth client secrets | OAuth client ID |

### 8.2 Secret Rotation

```csharp
// Use IOptionsMonitor for secrets that may rotate
public sealed class EmailService
{
    private readonly IOptionsMonitor<SendGridOptions> _options;

    public EmailService(IOptionsMonitor<SendGridOptions> options)
    {
        _options = options;
    }

    public async Task SendAsync(Email email)
    {
        // Always gets current value (supports rotation)
        var apiKey = _options.CurrentValue.ApiKey;
        // ...
    }
}
```

### 8.3 Never Log Secrets

```csharp
// BAD - logs the secret
_logger.LogInformation("Using API key: {ApiKey}", options.ApiKey);

// GOOD - logs that secret is configured
_logger.LogInformation("SendGrid API key configured: {IsConfigured}",
    !string.IsNullOrEmpty(options.ApiKey));
```

---

## 9. Hot-Reload Support

### 9.1 What Supports Hot-Reload

| Configuration Source | Hot-Reload | Notes |
|---------------------|------------|-------|
| appsettings.json | ✅ Yes | `reloadOnChange: true` (default) |
| appsettings.{Env}.json | ✅ Yes | Same as above |
| User Secrets | ✅ Yes | In Development only |
| Environment Variables | ❌ No | Requires restart |
| Aspire Parameters | ❌ No | Requires restart |
| K8s ConfigMaps | ⚠️ Partial | Depends on mount type |

### 9.2 Enabling Hot-Reload

```csharp
// Default in .NET - hot-reload is enabled
var builder = WebApplication.CreateBuilder(args);

// Explicit configuration (if needed)
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
                 optional: true, reloadOnChange: true);
```

### 9.3 Reacting to Configuration Changes

```csharp
public sealed class FeatureFlagService : IDisposable
{
    private readonly IOptionsMonitor<FeatureOptions> _options;
    private readonly IDisposable? _changeListener;

    public FeatureFlagService(IOptionsMonitor<FeatureOptions> options, ILogger<FeatureFlagService> logger)
    {
        _options = options;

        _changeListener = _options.OnChange(newOptions =>
        {
            logger.LogInformation(
                "Feature flags updated. EnableNewCheckout: {EnableNewCheckout}",
                newOptions.EnableNewCheckoutFlow);
        });
    }

    public void Dispose() => _changeListener?.Dispose();
}
```

---

## 10. Validation Checklist

### Development Environment

- [ ] User Secrets initialized for each service (`dotnet user-secrets init`)
- [ ] All sensitive values in User Secrets, not appsettings
- [ ] `ValidateOnStart()` configured for critical options
- [ ] Application starts without configuration errors

### Docker Compose Environment

- [ ] `.env` file excluded from git (`.gitignore`)
- [ ] Environment variables properly mapped in docker-compose
- [ ] Secrets not visible in `docker inspect` output
- [ ] Application connects to all dependencies

### Production Readiness

- [ ] No secrets in appsettings.json or source control
- [ ] Secret backend selected (Key Vault / K8s Secrets)
- [ ] Configuration validation enabled
- [ ] Logging does not expose sensitive values

---

## 11. Related Documents

- [Aspire Orchestration](./aspire-orchestration.md) - Service discovery, connection string injection
- [Error Handling](./error-handling.md) - Configuration validation error responses
- [Shared Projects](./shared-projects.md) - Configuration class locations
