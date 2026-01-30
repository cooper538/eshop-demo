# Task 5: EShop.Common (Layered)

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | ✅ completed |
| Dependencies | task-01, task-02 |

## Summary
Implement shared infrastructure split into three layered projects following Clean Architecture.

## Scope

### EShop.Common.Application
- [x] Exception hierarchy in `Exceptions/`:
  - `ApplicationException.cs` - base exception
  - `NotFoundException.cs`
  - `ValidationException.cs`
  - `ConflictException.cs`
- [x] MediatR behaviors in `Behaviors/`:
  - `ValidationBehavior.cs` - FluentValidation integration
  - `LoggingBehavior.cs` - request/response logging
  - `UnitOfWorkBehavior.cs` - auto SaveChanges after commands
  - `CommandTrackingBehavior.cs` - EF tracking on for commands
  - `QueryTrackingBehavior.cs` - EF no-tracking for queries
  - `DomainEventDispatchHelper.cs` - domain event publishing helper
- [x] Correlation context in `Correlation/`:
  - `ICorrelationIdAccessor.cs` - interface for getting correlation ID
  - `CorrelationIdAccessor.cs` - AsyncLocal implementation
  - `CorrelationContext.cs` - scoped context
  - `CorrelationIdConstants.cs` - header names
- [x] CQRS abstractions in `Cqrs/`:
  - `ICommand.cs` - MediatR command marker
  - `IQuery.cs` - MediatR query marker
- [x] Domain event infrastructure in `Events/`:
  - `IDomainEventDispatcher.cs`
  - `DomainEventNotification.cs`
  - `MediatRDomainEventDispatcher.cs`
- [x] Data abstractions in `Data/`:
  - `IUnitOfWork.cs`
  - `IChangeTrackerAccessor.cs`
- [x] Services:
  - `DateTimeProvider.cs` - IDateTimeProvider implementation
- [x] `Extensions/ServiceCollectionExtensions.cs` - DI registration

### EShop.Common.Api
- [x] HTTP middleware in `Http/`:
  - `GlobalExceptionHandler.cs` - IExceptionHandler
  - `CorrelationIdMiddleware.cs` - extracts/generates correlation ID
- [x] gRPC server interceptors in `Grpc/`:
  - `CorrelationIdServerInterceptor.cs` - metadata to context
  - `GrpcLoggingInterceptor.cs` - request/response logging
  - `GrpcExceptionInterceptor.cs` - exception to gRPC status mapping
  - `GrpcValidationInterceptor.cs` - FluentValidation for gRPC
- [x] `Extensions/ServiceCollectionExtensions.cs` - DI registration

### EShop.Common.Infrastructure
- [x] MassTransit correlation filters in `Correlation/MassTransit/`:
  - `CorrelationIdPublishFilter.cs`
  - `CorrelationIdConsumeFilter.cs`
  - `CorrelationIdSendFilter.cs`
  - `MassTransitCorrelationExtensions.cs`
- [x] EF Core configurations in `Data/`:
  - `EntityConfiguration.cs` - base Entity config
  - `AggregateRootConfiguration.cs` - Version as concurrency token
  - `RemoveEntitySuffixConvention.cs` - table naming convention
- [x] `Extensions/MessagingExtensions.cs` - MassTransit setup
- [x] `Extensions/ValidationExtensions.cs` - FluentValidation DI

## Related Specs
- -> [shared-projects.md](../../high-level-specs/shared-projects.md) (Section: 3.4 - EShop.Common)
- -> [error-handling.md](../../high-level-specs/error-handling.md) (Section: 3, 4, 5)
- -> [correlation-id-flow.md](../../high-level-specs/correlation-id-flow.md)

---
## Notes
- Original spec had single EShop.Common project, split into 3 for Clean Architecture compliance
- Dependency chain: SharedKernel <- Common.Application <- Common.Api/Infrastructure
- UnitOfWork pattern auto-saves after commands, dispatches domain events
- Correlation ID flows through HTTP -> gRPC -> MassTransit automatically
