# Code Guidelines

Project-specific C# standards for AI agent.

---

## Naming (project-specific)

| Element | Convention | Example |
|---------|------------|---------|
| Enums | prefix `E`, suffix `Type` | `EOrderStateType` |

- Include units in variable names: `pollIntervalMs`, `fileSizeGb`
- Always use `var`

---

## File Organization

- Generic file naming: `Something.cs` (non-generic), `Something(T).cs` (generic)
- Group interfaces in `Interfaces/` folder if needed
- Group enums in `Enumerations/` folder if needed

---

## Class Structure Order

```
1. Constant Fields
2. Fields
3. Constructors
4. Properties
5. Public Methods
6. Private Methods
```

---

## Documentation

**Controller methods** - XML docs for Swagger:
```csharp
/// <summary>Retrieves a product by its ID.</summary>
/// <param name="id">The ID of the product</param>
/// <returns>The product DTO</returns>
public async Task<ActionResult<ProductResponseDto>> GetProduct(string id)
```

**TODO comments** - include ticket number:
```csharp
// TODO: Will be implemented in DEV-123
```

---

## Forbidden

- `dynamic` keyword (except in tests)
- Nested ternary operators
- Magic numbers