# Configuration Management

## Overview

This specification defines the configuration management approach for EShop microservices. The system uses YAML-based configuration with Options pattern and runtime validation.

## Key Principles

1. **C# as Source of Truth** - Configuration structure defined in C# classes with DataAnnotations
2. **YAML for Readability** - Human-readable config files with native comment support
3. **Runtime Validation** - ValidateOnStart() ensures fail-fast on invalid config
4. **Explicit Naming** - Service-specific file names prevent mixing configs in monorepo
5. **Environment Hierarchy** - Base config with environment-specific overrides

## File Naming Convention

```
{service}.settings.yaml                    # Base configuration
{service}.settings.{Environment}.yaml      # Environment override
```

**Examples:**
```
src/Services/Product/Product.API/
├── product.settings.yaml                  # Base (always loaded)
├── product.settings.Development.yaml      # Dev overrides
└── product.settings.Production.yaml       # Prod overrides

src/Services/Order/Order.API/
├── order.settings.yaml
├── order.settings.Development.yaml
└── order.settings.Production.yaml
```

## Configuration Structure

### YAML File Format

```yaml
# yaml-language-server: $schema=./product.schema.json
# Product Service Configuration

Product:
  Service:
    Name: "Product"
    Version: "1.0.0"

  Database:
    CommandTimeout: 30
    EnableRetry: true
    MaxRetryCount: 3

  Cache:
    Enabled: true
    ExpirationMinutes: 60

  Logging:
    Level: "Information"
```

### C# Settings Class

```csharp
public class ProductSettings
{
    public const string SectionName = "Product";

    [Required]
    public ServiceInfo Service { get; init; } = new();

    [Required]
    public DatabaseSettings Database { get; init; } = new();

    public CacheSettings Cache { get; init; } = new();

    public LoggingSettings Logging { get; init; } = new();
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
    public int CommandTimeout { get; init; } = 30;

    public bool EnableRetry { get; init; } = true;

    [Range(1, 10)]
    public int MaxRetryCount { get; init; } = 3;
}

public class CacheSettings
{
    public bool Enabled { get; init; } = true;

    [Range(1, 1440)]
    public int ExpirationMinutes { get; init; } = 60;
}

public class LoggingSettings
{
    public string Level { get; init; } = "Information";
}
```

## Loading Priority

Configuration is loaded in this order (later overrides earlier):

```
1. {service}.settings.yaml           # Base config
      ↓ override
2. {service}.settings.{Env}.yaml     # Environment-specific
      ↓ override
3. Environment variables             # Aspire/K8s/Docker injected
      ↓ override
4. Command line arguments            # Runtime overrides
```

## Implementation

### NuGet Packages

```xml
<ItemGroup>
  <PackageReference Include="NetEscapades.Configuration.Yaml" />
</ItemGroup>
```

### Program.cs Setup

```csharp
var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment.EnvironmentName;

// Load YAML configuration
builder.Configuration
    .AddYamlFile("product.settings.yaml", optional: false, reloadOnChange: true)
    .AddYamlFile($"product.settings.{env}.yaml", optional: true, reloadOnChange: true);

// Bind and validate settings
builder.Services
    .AddOptions<ProductSettings>()
    .BindConfiguration(ProductSettings.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();  // Fail fast if config is invalid

builder.AddServiceDefaults();

var app = builder.Build();
```

## IDE Support

For YAML intellisense, add schema reference as first line:

```yaml
# yaml-language-server: $schema=./product.schema.json
Product:
  ...
```

Schema can be generated manually or via build tool (see Future Extensions).

**VS Code:** Install `redhat.vscode-yaml` extension.
**Rider:** Auto-detects `.schema.json` files.

## Validation

`ValidateOnStart()` ensures application fails fast with clear error message if:
- Required fields are missing
- Values are out of range
- Type mismatches occur

```
Unhandled exception: OptionsValidationException:
  DataAnnotation validation failed for 'ProductSettings':
    The CommandTimeout field must be between 1 and 300.
```

## Environment Variables Override

For Aspire/Kubernetes deployment, environment variables can override any setting:

```bash
# Convention: Section__Property (double underscore)
Product__Database__CommandTimeout=60
Product__Cache__Enabled=false
```

In Aspire AppHost:

```csharp
builder.AddProject<Product_API>("product-api")
    .WithEnvironment("Product__Database__CommandTimeout", "60");
```

## Secrets Management

**Never store secrets in YAML files.**

Use:
- **Development**: User Secrets (`dotnet user-secrets`)
- **Production**: Environment variables, Azure Key Vault, or Aspire secrets

```csharp
// Secrets loaded separately
builder.Configuration.AddUserSecrets<Program>(optional: true);
```

## Migration from appsettings.json

1. Rename `appsettings.json` → `{service}.settings.yaml`
2. Convert JSON to YAML format
3. Add schema reference comment
4. Update Program.cs to use `AddYamlFile()`
5. Generate schema from settings class
6. Remove old JSON files

## Summary

| Aspect | Approach |
|--------|----------|
| Format | YAML |
| Naming | `{service}.settings.{env}.yaml` |
| Schema | JSON Schema from C# classes |
| Validation | DataAnnotations + ValidateOnStart() |
| IDE Support | YAML Language Server + schema |
| Secrets | Environment variables / Key Vault |
| Override | Env vars with `__` separator |
