# Task 08: YAML Configuration

## Metadata
| Key | Value |
|-----|-------|
| ID | task-08 |
| Status | 🔵 in_progress |
| Dependencies | task-01 |

## Summary
Implement YAML-based configuration with Options pattern and runtime validation via ValidateOnStart().

## Scope
- [ ] Add NuGet package to Product.API (NetEscapades.Configuration.Yaml)
- [ ] Create ProductSettings.cs with Options pattern and DataAnnotations
- [ ] Create product.settings.yaml (base configuration)
- [ ] Create product.settings.Development.yaml (development overrides)
- [ ] Create product.settings.Production.yaml (production overrides)
- [ ] Configure Program.cs to load YAML config with ValidateOnStart()
- [ ] Add YAML schema reference comment for IDE intellisense
- [ ] Verify application fails fast on invalid config

## Configuration Structure
```yaml
# product.settings.yaml
Product:
  Service:
    Name: "Product"
    Version: "1.0.0"
  Database:
    CommandTimeout: 30
    EnableRetry: true
  Cache:
    ExpirationMinutes: 60
  Logging:
    Level: "Information"
```

## Related Specs
- → [configuration-management.md](../../high-level-specs/configuration-management.md)

---
## Notes
(Updated during implementation)
