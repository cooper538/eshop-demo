# Task 02: Endpoint Conventions for Basic Tier

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | pending |
| Dependencies | task-01 |

## Summary
Configure MassTransit endpoint conventions for Azure Service Bus Basic tier which only supports queues (no topics/subscriptions).

## Scope
- [ ] Create `ServiceBusBasicTierConfigurator` class for queue-only routing
- [ ] Configure `EndpointConvention` mappings for all integration events
- [ ] Map each event type to its dedicated queue (queue-per-event pattern)
- [ ] Add `SendEndpointConfigurationExtensions` for simplified queue mapping
- [ ] Update `ServiceBusMessagingExtensions` to use endpoint conventions

## Implementation Notes

Azure Service Bus Basic tier does not support topics - only queues. MassTransit needs explicit endpoint conventions to route messages correctly.

```csharp
// ServiceBusBasicTierConfigurator.cs
public static class ServiceBusBasicTierConfigurator
{
    public static void ConfigureEndpointConventions(
        IServiceBusBusFactoryConfigurator cfg,
        string serviceBusConnectionString)
    {
        // Map each event to its dedicated queue
        // Queue names match those defined in azure-infrastructure.md
        EndpointConvention.Map<OrderConfirmed>(
            new Uri($"queue:order-confirmed"));
        EndpointConvention.Map<OrderRejected>(
            new Uri($"queue:order-rejected"));
        EndpointConvention.Map<OrderCancelled>(
            new Uri($"queue:order-cancelled"));
        EndpointConvention.Map<StockLow>(
            new Uri($"queue:stock-low"));
        EndpointConvention.Map<StockReservationExpired>(
            new Uri($"queue:stock-reservation-expired"));
    }
}

// In consumer configuration
cfg.ReceiveEndpoint("order-confirmed", e =>
{
    e.ConfigureConsumer<OrderConfirmedConsumer>(context);
});
```

## Queue Mapping

| Event | Queue Name |
|-------|------------|
| OrderConfirmed | order-confirmed |
| OrderRejected | order-rejected |
| OrderCancelled | order-cancelled |
| StockLow | stock-low |
| StockReservationExpired | stock-reservation-expired |

## Files to Create/Modify

| Action | File |
|--------|------|
| CREATE | `src/Common/EShop.Common.Infrastructure/Messaging/ServiceBusBasicTierConfigurator.cs` |
| MODIFY | `src/Common/EShop.Common.Infrastructure/Messaging/ServiceBusMessagingExtensions.cs` |

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 4.2 Service Bus Queues, 4.3 Basic Tier Limitations)
- -> [messaging-communication.md](../high-level-specs/messaging-communication.md) (Section: 2. Integration Events Catalog)

---
## Notes
(Updated during implementation)
