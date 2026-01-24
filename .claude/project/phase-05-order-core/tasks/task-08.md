# Task 08: YAML Configuration

## Metadata
| Key | Value |
|-----|-------|
| ID | task-08 |
| Status | :white_circle: pending |
| Dependencies | task-01 |

## Summary
Implement YAML-based configuration with Options pattern and runtime validation.

## Scope

### NuGet Package
- [ ] Add `NetEscapades.Configuration.Yaml` to Order.API

### OrderSettings Class
- [ ] Create `OrderSettings.cs` in Order.API/Configuration
  ```csharp
  public class OrderSettings
  {
      public const string SectionName = "Order";

      [Required]
      public ServiceInfo Service { get; init; } = new();

      [Required]
      public DatabaseSettings Database { get; init; } = new();
  }

  public class ServiceInfo
  {
      [Required]
      [StringLength(50)]
      public string Name { get; init; } = string.Empty;

      [Required]
      public string Version { get; init; } = "1.0.0";
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

### YAML Configuration Files
- [ ] Create `order.settings.yaml` (base configuration)
  ```yaml
  # yaml-language-server: $schema=./Configuration/order.settings.schema.json

  Order:
    Service:
      Name: "Order"
      Version: "1.0.0"

    Database:
      CommandTimeoutSec: 30
      EnableRetry: true
      MaxRetryCount: 3
  ```

- [ ] Create `order.settings.Development.yaml` (dev overrides)
  ```yaml
  Order:
    Database:
      CommandTimeoutSec: 60
      MaxRetryCount: 1
  ```

### JSON Schema (for IDE intellisense)
- [ ] Create `order.settings.schema.json` in Configuration/
  - Define schema matching OrderSettings structure
  - Enable YAML intellisense in IDEs

### Program.cs Integration
- [ ] Load YAML configuration
  ```csharp
  builder.Configuration
      .AddYamlFile("order.settings.yaml", optional: false, reloadOnChange: true)
      .AddYamlFile($"order.settings.{env}.yaml", optional: true, reloadOnChange: true);
  ```

- [ ] Configure Options with validation
  ```csharp
  builder.Services
      .AddOptions<OrderSettings>()
      .BindConfiguration(OrderSettings.SectionName)
      .ValidateDataAnnotations()
      .ValidateOnStart();  // Fail fast on invalid config
  ```

## Reference Implementation
See `ProductSettings` and `product.settings.yaml` in Products.API

## Related Specs
- → [configuration-management.md](../../high-level-specs/configuration-management.md)

---
## Notes
(Updated during implementation)
