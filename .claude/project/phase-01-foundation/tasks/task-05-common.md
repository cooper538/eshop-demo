# Task 5: EShop.Common

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | ✅ completed |
| Dependencies | task-01, task-02 |

## Summary
Implementovat sdílenou infrastrukturu - middleware, behaviors, exception handling.

## Scope
- [ ] Vytvořit projekt `EShop.Common` v `src/Common/EShop.Common/`
- [ ] Implementovat Exception hierarchy v `Exceptions/`:
  - `ApplicationException.cs` - base exception
  - `NotFoundException.cs`
  - `ValidationException.cs`
  - `ConflictException.cs`
- [ ] Implementovat `Middleware/GlobalExceptionHandler.cs` - IExceptionHandler
- [ ] Implementovat MediatR behaviors v `Behaviors/`:
  - `ValidationBehavior.cs`
  - `LoggingBehavior.cs`
- [ ] Implementovat Correlation context v `Correlation/`:
  - `ICorrelationContext.cs`
  - `CorrelationContext.cs`
  - `CorrelationIdConstants.cs`
- [ ] Implementovat `Middleware/CorrelationIdMiddleware.cs`
- [ ] Implementovat gRPC interceptory v `Grpc/`:
  - `CorrelationIdClientInterceptor.cs`
  - `CorrelationIdServerInterceptor.cs`
- [ ] Implementovat `Extensions/ServiceCollectionExtensions.cs` - DI registrace

## Related Specs
- → [shared-projects.md](../../high-level-specs/shared-projects.md) (Section: 3.4 - EShop.Common)
- → [error-handling.md](../../high-level-specs/error-handling.md) (Section: 3, 4, 5 - Exception hierarchy a middleware)

---
## Notes
(Updated during implementation)
