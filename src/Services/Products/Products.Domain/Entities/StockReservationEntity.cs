using EShop.SharedKernel.Domain;
using Products.Domain.Enums;
using Products.Domain.Events;

namespace Products.Domain.Entities;

public class StockReservationEntity : Entity
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(15);

    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public DateTime ReservedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? ReleasedAt { get; private set; }
    public EReservationStatus Status { get; private set; }
    public uint RowVersion { get; private set; }

    public bool IsExpired => Status == EReservationStatus.Active && DateTime.UtcNow >= ExpiresAt;

    // EF Core constructor
    private StockReservationEntity() { }

    public static StockReservationEntity Create(Guid orderId, Guid productId, int quantity)
    {
        var now = DateTime.UtcNow;

        var reservation = new StockReservationEntity
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
            ReservedAt = now,
            ExpiresAt = now.Add(DefaultTtl),
            ReleasedAt = null,
            Status = EReservationStatus.Active,
        };

        reservation.AddDomainEvent(new StockReservedDomainEvent(orderId, productId, quantity, now));

        return reservation;
    }

    public void Release()
    {
        if (Status != EReservationStatus.Active)
        {
            throw new InvalidOperationException(
                $"Cannot release reservation in status {Status}. Only Active reservations can be released."
            );
        }

        Status = EReservationStatus.Released;
        ReleasedAt = DateTime.UtcNow;

        AddDomainEvent(
            new StockReleasedDomainEvent(OrderId, ProductId, Quantity, ReleasedAt.Value)
        );
    }

    public void Expire()
    {
        if (Status != EReservationStatus.Active)
        {
            throw new InvalidOperationException(
                $"Cannot expire reservation in status {Status}. Only Active reservations can be expired."
            );
        }

        Status = EReservationStatus.Expired;

        AddDomainEvent(new StockReservationExpiredDomainEvent(OrderId, ProductId, Quantity));
    }
}
