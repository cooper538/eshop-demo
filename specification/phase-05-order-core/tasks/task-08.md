# Task 08: YAML Configuration

## Metadata
| Key | Value |
|-----|-------|
| ID | task-08 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Summary
Implement YAML-based configuration with Options pattern and runtime validation.

## Scope

### OrderSettings Class - IMPLEMENTED
- [x] `OrderSettings.cs` in Order.API/Configuration
  - `ServiceInfo Service` (from EShop.Common.Application.Configuration)
  - `DatabaseSettings Database`
- [x] `DatabaseSettings` class
  - `CommandTimeoutSec` (default 30, range 1-300)
  - `EnableRetry` (default true)
  - `MaxRetryCount` (default 3, range 1-10)

### YAML Configuration Files - IMPLEMENTED
- [x] `order.settings.yaml` (base configuration)
- [x] `order.settings.Development.yaml` (dev overrides)
- [x] `order.settings.Production.yaml` (prod overrides)

### JSON Schema - IMPLEMENTED
- [x] `order.settings.schema.json` in Configuration/
  - Enables YAML intellisense in IDEs

### Program.cs Integration - IMPLEMENTED
- [x] YAML loading via `AddYamlConfiguration("order")` extension
- [x] Options validation with `ValidateDataAnnotations()` and `ValidateOnStart()`

## Actual Implementation

**order.settings.yaml:**
```yaml
Order:
  Service:
    Name: "Order"
  Database:
    CommandTimeoutSec: 30
    EnableRetry: true
    MaxRetryCount: 3

ServiceClients:
  TimeoutSeconds: 30
  ProductService:
    Url: "https://product-service"
```

**OrderSettings.cs:**
```csharp
public class OrderSettings
{
    public const string SectionName = "Order";

    [Required]
    public ServiceInfo Service { get; init; } = new();

    [Required]
    public DatabaseSettings Database { get; init; } = new();
}

public class DatabaseSettings
{
    [Range(1, 300)]
    public int CommandTimeoutSec { get; init; } = 30;

    public bool EnableRetry { get; init; } = true;

    [Range(1, 10)]
    public int MaxRetryCount { get; init; } = 3;
}
```

## File Structure
```
Order.API/
├── Configuration/
│   ├── OrderSettings.cs
│   └── order.settings.schema.json
├── order.settings.yaml
├── order.settings.Development.yaml
└── order.settings.Production.yaml
```

## Key Details
- Uses shared `ServiceInfo` class from EShop.Common.Application.Configuration
- Includes `ServiceClients` section for Product Service gRPC configuration

## Reference Implementation
See `ProductSettings` and `product.settings.yaml` in Products.API

## Related Specs
- -> [configuration-management.md](../../high-level-specs/configuration-management.md)

---
## Notes
(Updated during implementation)
