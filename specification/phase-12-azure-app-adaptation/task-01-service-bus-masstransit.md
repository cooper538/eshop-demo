# Task 01: Service Bus MassTransit Configuration

## Metadata
| Key | Value |
|-----|-------|
| ID | task-01 |
| Status | pending |
| Dependencies | - |

## Summary
Create `AddMessagingAzure<T>()` extension method for Azure Service Bus with MassTransit, parallel to existing `AddMessaging<T>()` for RabbitMQ.

## Scope
- [ ] Add `Azure.Messaging.ServiceBus` and `MassTransit.Azure.ServiceBus.Core` packages to `Directory.Packages.props`
- [ ] Create `ServiceBusMessagingExtensions.cs` in `EShop.Common.Infrastructure/Messaging/`
- [ ] Implement `AddMessagingAzure<TDbContext>()` extension method
- [ ] Configure MassTransit to use Azure Service Bus transport
- [ ] Use `DefaultAzureCredential` for authentication (supports both local dev and Managed Identity)
- [ ] Configure retry policy matching RabbitMQ configuration (1s, 5s, 15s intervals)
- [ ] Add CorrelationId filter for distributed tracing
- [ ] Configure Outbox pattern integration (same as RabbitMQ setup)

## Implementation Notes

```csharp
// ServiceBusMessagingExtensions.cs
public static IHostApplicationBuilder AddMessagingAzure<TDbContext>(
    this IHostApplicationBuilder builder,
    string serviceName,
    Action<IBusRegistrationConfigurator>? configureBus = null)
    where TDbContext : DbContext
{
    builder.Services.AddMassTransit(x =>
    {
        configureBus?.Invoke(x);

        x.SetKebabCaseEndpointNameFormatter();

        x.AddEntityFrameworkOutbox<TDbContext>(o =>
        {
            o.UsePostgres();
            o.UseBusOutbox();
        });

        x.UsingAzureServiceBus((context, cfg) =>
        {
            var connectionString = builder.Configuration
                .GetConnectionString("servicebus");

            cfg.Host(connectionString);

            cfg.UseMessageRetry(r => r.Intervals(
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(15)));

            // CorrelationId propagation
            cfg.UseSendFilter(typeof(CorrelationIdSendFilter<>), context);
            cfg.UsePublishFilter(typeof(CorrelationIdPublishFilter<>), context);
            cfg.UseConsumeFilter(typeof(CorrelationIdConsumeFilter<>), context);

            cfg.ConfigureEndpoints(context);
        });
    });

    return builder;
}
```

## Files to Create/Modify

| Action | File |
|--------|------|
| MODIFY | `Directory.Packages.props` |
| CREATE | `src/Common/EShop.Common.Infrastructure/Messaging/ServiceBusMessagingExtensions.cs` |

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 4. Messaging Resources)
- -> [messaging-communication.md](../high-level-specs/messaging-communication.md) (Section: 8. MassTransit Configuration)

---
## Notes
(Updated during implementation)
