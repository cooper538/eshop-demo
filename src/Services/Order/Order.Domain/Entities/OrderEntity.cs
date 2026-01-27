using EShop.SharedKernel.Domain;
using Order.Domain.Enums;
using Order.Domain.Exceptions;

namespace Order.Domain.Entities;

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
        IEnumerable<OrderItem> items
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
            CreatedAt = DateTime.UtcNow,
            Items = itemList,
        };
    }

    public void Confirm()
    {
        if (Status != EOrderStatus.Created)
        {
            throw new InvalidOrderStateException(Status, EOrderStatus.Confirmed);
        }

        Status = EOrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(string reason)
    {
        if (Status != EOrderStatus.Created)
        {
            throw new InvalidOrderStateException(Status, EOrderStatus.Rejected);
        }

        Status = EOrderStatus.Rejected;
        RejectionReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (Status != EOrderStatus.Confirmed)
        {
            throw new InvalidOrderStateException(Status, EOrderStatus.Cancelled);
        }

        Status = EOrderStatus.Cancelled;
        RejectionReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }
}
