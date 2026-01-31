using EShop.Order.Domain.Enums;
using EShop.Order.Domain.Events;
using EShop.Order.Domain.Exceptions;
using EShop.SharedKernel.Domain;

namespace EShop.Order.Domain.Entities;

public class OrderEntity : AggregateRoot
{
    public Guid CustomerId { get; private set; }
    public string CustomerEmail { get; private set; } = null!;
    public EOrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private List<OrderItem> _items = [];
    public IReadOnlyList<OrderItem> Items
    {
        get => _items.AsReadOnly();
        private set => _items = value.ToList();
    }

    // EF Core constructor
    private OrderEntity() { }

    public static OrderEntity Create(
        Guid customerId,
        string customerEmail,
        IEnumerable<OrderItem> items,
        DateTime createdAt
    )
    {
        var itemList = items.ToList();

        return new OrderEntity
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            CustomerEmail = customerEmail,
            Status = EOrderStatus.Created,
            TotalAmount = itemList.Sum(i => i.LineTotal),
            CreatedAt = createdAt,
            Items = itemList,
        };
    }

    public void Confirm(DateTime occurredAt)
    {
        if (Status != EOrderStatus.Created)
        {
            throw new InvalidOrderStateException(Status, EOrderStatus.Confirmed);
        }

        Status = EOrderStatus.Confirmed;
        UpdatedAt = occurredAt;
        IncrementVersion();

        AddDomainEvent(
            new OrderConfirmedDomainEvent(
                Id,
                CustomerId,
                CustomerEmail,
                TotalAmount,
                Items
                    .Select(i => new OrderItemInfo(
                        i.ProductId,
                        i.ProductName,
                        i.Quantity,
                        i.UnitPrice
                    ))
                    .ToList()
            )
            {
                OccurredOn = occurredAt,
            }
        );
    }

    public void Reject(string reason, DateTime occurredAt)
    {
        if (Status != EOrderStatus.Created)
        {
            throw new InvalidOrderStateException(Status, EOrderStatus.Rejected);
        }

        Status = EOrderStatus.Rejected;
        RejectionReason = reason;
        UpdatedAt = occurredAt;
        IncrementVersion();

        AddDomainEvent(
            new OrderRejectedDomainEvent(Id, CustomerId, CustomerEmail, reason)
            {
                OccurredOn = occurredAt,
            }
        );
    }

    public void Cancel(string reason, DateTime occurredAt)
    {
        if (Status != EOrderStatus.Confirmed)
        {
            throw new InvalidOrderStateException(Status, EOrderStatus.Cancelled);
        }

        Status = EOrderStatus.Cancelled;
        RejectionReason = reason;
        UpdatedAt = occurredAt;
        IncrementVersion();

        AddDomainEvent(
            new OrderCancelledDomainEvent(Id, CustomerId, CustomerEmail, reason)
            {
                OccurredOn = occurredAt,
            }
        );
    }
}
