using EShop.SharedKernel.Domain;
using Products.Domain.Enums;

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
    public EReservationStatusType Status { get; private set; }

    public bool IsExpired =>
        Status == EReservationStatusType.Active && DateTime.UtcNow >= ExpiresAt;

    // EF Core constructor
    private StockReservationEntity() { }

    public static StockReservationEntity Create(Guid orderId, Guid productId, int quantity)
    {
        var now = DateTime.UtcNow;

        return new StockReservationEntity
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
            ReservedAt = now,
            ExpiresAt = now.Add(DefaultTtl),
            ReleasedAt = null,
            Status = EReservationStatusType.Active,
        };
    }

    public void Release()
    {
        if (Status != EReservationStatusType.Active)
        {
            throw new InvalidOperationException(
                $"Cannot release reservation in status {Status}. Only Active reservations can be released."
            );
        }

        Status = EReservationStatusType.Released;
        ReleasedAt = DateTime.UtcNow;
    }

    public void Expire()
    {
        if (Status != EReservationStatusType.Active)
        {
            throw new InvalidOperationException(
                $"Cannot expire reservation in status {Status}. Only Active reservations can be expired."
            );
        }

        Status = EReservationStatusType.Expired;
    }
}
