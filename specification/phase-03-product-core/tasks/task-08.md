# Task 08: YAML Configuration

## Metadata
| Key | Value |
|-----|-------|
| ID | task-08 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Summary
Implement YAML-based configuration with Options pattern, DataAnnotations validation, and stock reservation settings.

## Scope
- [x] Use AddYamlConfiguration() extension from EShop.Common.Api
- [x] Create ProductSettings.cs with Options pattern and DataAnnotations
- [x] Create StockReservationOptions.cs implementing IStockReservationOptions
- [x] Create product.settings.yaml (base configuration)
- [x] Create product.settings.Development.yaml (development overrides)
- [x] Create product.settings.Production.yaml (production overrides)
- [x] Configure Program.cs to load YAML config with ValidateOnStart()
- [x] Add YAML schema reference comment for IDE intellisense
- [x] Verify application fails fast on invalid config

## Related Specs
- → [configuration-management.md](../../high-level-specs/configuration-management.md)

---
## Notes
**Actual configuration structure** (product.settings.yaml):
```yaml
# yaml-language-server: $schema=./Configuration/product.settings.schema.json

AllowedHosts: "*"

Logging:
  LogLevel:
    Default: "Information"
    Microsoft.AspNetCore: "Warning"

Product:
  Service:
    Name: "Products"

  Database:
    CommandTimeoutSec: 30
    EnableRetry: true
    MaxRetryCount: 3

  Cache:
    Enabled: true
    ExpirationMin: 60

  StockReservation:
    DefaultDurationMinutes: 5
    Expiration:
      CheckIntervalMinutes: 1
      BatchSize: 50
```

**Options classes**:
- `ProductSettings` - main configuration bound to "Product" section
- `StockReservationOptions` - implements `IStockReservationOptions` interface for background job

**Registration**:
```csharp
builder.AddYamlConfiguration("product");  // loads product.settings.{Environment}.yaml

builder.Services.AddOptions<ProductSettings>()
    .BindConfiguration(ProductSettings.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<IStockReservationOptions, StockReservationOptions>();
```

**Stock reservation options** used by `StockReservationExpirationJob`:
- `DefaultDuration` - how long reservations are valid
- `Expiration.CheckInterval` - how often to check for expired reservations
- `Expiration.BatchSize` - how many reservations to process per batch
